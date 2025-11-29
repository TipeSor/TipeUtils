using System.Reflection;
using TipeUtils.Collections;
using TipeUtils.Formatting;
using TipeUtils.Parsing;
using TipeUtils.Reflection;

namespace TipeUtils.IO
{
    public sealed class Input : IDisposable
    {
        public TextReader Stream { get; }
        private readonly bool _leaveOpen;
        private bool _disposed;
        private bool _canPopulate = true;
        public bool CanPopulate
        {
            get { lock (_syncLock) return _canPopulate; }
            set { lock (_syncLock) _canPopulate = value; }
        }

        private static readonly Dictionary<Type, InputElement[]> elementMap = [];
        private static bool _initialized;

        private LazyQueue<string> Tokens { get; set; } = new();

        private static readonly object _initLock = new();
        internal readonly object _syncLock = new();

        public Input() :
            this(new StreamReader(Console.OpenStandardInput(), leaveOpen: true), true)
        { }

        public Input(string path) :
            this(new StreamReader(path), false)
        { }

        public Input(TextReader reader, bool leaveOpen)
        {
            EnsureInitialized();
            Stream = reader;
            _leaveOpen = leaveOpen;
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_initLock)
            {
                if (!_initialized)
                    Initialize();
            }
        }

        private static void Initialize()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            IEnumerable<Type> types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(static asm => asm.GetTypes())
                .Where(static t => t.HasAttribute<ComplexTypeAttribute>());

            foreach (Type type in types)
            {
                List<(InputElement element, int index)> elements = [];

                foreach (FieldInfo f in type.GetFields(flags))
                {
                    if (f.GetCustomAttribute<InputElementAttribute>() is { } a)
                        elements.Add((new InputElement(f.Name, f.FieldType, false), a.Index));
                }

                foreach (PropertyInfo p in type.GetProperties(flags))
                {
                    if (p.GetCustomAttribute<InputElementAttribute>() is { } a)
                        elements.Add((new InputElement(p.Name, p.PropertyType, true), a.Index));
                }

                elements.Sort(static (a, b) => a.index.CompareTo(b.index));

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].index != i)
                        throw new InvalidOperationException($"{type.Name} invalid element indexes");
                }

                if (elements.Count == 0)
                    throw new InvalidOperationException($"{type.Name} has no valid input elements.");

                elementMap[type] = [.. elements.Select(static e => e.element)];
            }

            _initialized = true;
        }

        public string ReadLine()
        {
            lock (_syncLock)
                return ReadLineUnsafe();
        }

        public void Clear()
        {
            lock (_syncLock)
                GetTokensUnsafe();
        }

        public string[] GetTokens()
        {
            lock (_syncLock)
                return GetTokensUnsafe();
        }

        public string? GetToken()
        {
            lock (_syncLock)
                return GetTokenUnsafe();
        }

        internal string ReadLineUnsafe()
        {
            return Stream.ReadLine() ?? string.Empty;
        }

        internal string[] GetTokensUnsafe()
        {
            List<string> tokens = [];
            Result<string> result;
            while (!(result = Tokens.Dequeue()).IsError)
                tokens.Add(result.Value);
            return [.. tokens];
        }

        internal string? GetTokenUnsafe()
        {
            Result<string> result;
            while ((result = Tokens.Dequeue()).IsError)
            {
                if (!PopulateTokensUnsafe())
                    return null;
            }
            return result.Value;
        }

        private bool PopulateTokensUnsafe()
        {
            if (!_canPopulate) return false;

            string input = ReadLineUnsafe();
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            Tokens.Enqueue(StringUtils.Split(input));
            return true;
        }

        public int ReadMany<T>(Span<T> buffer)
            where T : notnull, new()
        {
            lock (_syncLock)
            {
                int i = 0;
                for (; i < buffer.Length; i++)
                {
                    Result<T> result = ReadUnsafe(typeof(T)).Cast<T>();
                    if (result.IsError) break;
                    buffer[i] = result.Value!;
                }
                return i;
            }
        }

        public int ReadMany<T>(T[] buffer, int index, int length)
            where T : notnull, new()
        {
            if ((uint)index > (uint)buffer.Length)
                return -1;

            if ((uint)length > (uint)(buffer.Length - index))
                return -1;

            return ReadMany(buffer.AsSpan(index, length));
        }

        public Result<object> Read(Type type)
        {
            lock (_syncLock)
            {
                return ReadUnsafe(type);
            }
        }

        public Result<T> Read<T>()
        {
            return Read(typeof(T)).Cast<T>();
        }

        internal Result<object> ReadUnsafe(Type type)
        {
            try
            {
                return elementMap.ContainsKey(type)
                        ? ReadComplexUnsafe(type)
                        : ReadSimpleUnsafe(type);
            }
            catch (Exception ex)
            {
                return Result<object>.Error(ex);
            }

        }

        internal Result<object> ReadSimpleUnsafe(Type type)
        {
            string? token = GetTokenUnsafe();
            if (token == null)
                return Result<object>.Error("Invalid token.");

            return TypeParser.Parse(token, type);
        }

        internal Result<object> ReadComplexUnsafe(Type type)
        {
            try
            {
                if (!elementMap.TryGetValue(type, out InputElement[]? elements))
                    throw new InvalidOperationException($"Input map for {type.Name} not found");

                object? obj = Activator.CreateInstance(type);

                if (obj == null)
                    return Result<object>.Error($"{type.Name} cannot be deserialized because it does not have a parameterless constructor.");

                InputElement? failedElement = null;

                for (int i = 0; i < elements.Length; i++)
                {
                    InputElement element = elements[i];
                    Type memberType = Nullable.GetUnderlyingType(element.MemberType) ?? element.MemberType;

                    Result<object> result = ReadUnsafe(memberType);

                    if (result.IsError)
                    {
                        failedElement = element;
                        for (int j = i + 1; j < elements.Length; j++)
                            GetToken();
                        break;
                    }

                    if (element.IsProperty)
                    {
                        PropertyInfo? prop = type.GetProperty(element.Name,
                            BindingFlags.Instance | BindingFlags.Public);
                        if (prop?.CanWrite == true)
                            prop.SetValue(obj, result.Value);
                    }
                    else
                    {
                        FieldInfo? field = type.GetField(element.Name);

                        if (field == null)
                        {
                            failedElement ??= element;
                            continue;
                        }

                        field.SetValue(obj, result.Value);
                    }
                }

                if (failedElement != null)
                    return Result<object>.Error($"Field `{failedElement.Name}` of type `{type.Name}` failed to parse.");

                return Result<object>.Ok(obj);

            }
            catch (Exception ex)
            {
                return Result<object>.Error(ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (_syncLock)
            {
                if (_disposed)
                    return;

                if (disposing && !_leaveOpen)
                {
                    Stream?.Dispose();
                }

                _disposed = true;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ComplexTypeAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InputElementAttribute(int index) : Attribute
    {
        public int Index { get; } = index;
    }

    internal record InputElement(string Name, Type MemberType, bool IsProperty);
}
