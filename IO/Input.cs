using System.Diagnostics.CodeAnalysis;
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
        private readonly bool _skipDispose;
        private bool _disposed;
        public bool CanPopulate { get; set; } = true;

        private static readonly Dictionary<Type, InputElement[]> elementMap = [];
        private static bool _initialized;

        private LazyQueue<string> Tokens { get; set; } = [];

        private static readonly object _staticLock = new();
        private readonly object _instanceLock = new();

        public Input() :
            this(new StreamReader(Console.OpenStandardInput(), leaveOpen: true), true)
        { }

        public Input(string path) :
            this(new StreamReader(path))
        { }

        public Input(TextReader reader, bool skipDispose = false)
        {
            if (!_initialized) Initialize();
            Stream = reader;
            _skipDispose = skipDispose;
        }

        private static void Initialize()
        {
            lock (_staticLock)
            {
                if (_initialized) return;

                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

                IEnumerable<Type> types = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .SelectMany(static asm => asm.GetTypes())
                                        .Where(static t => t != null && t.HasAttribute<ComplexTypeAttribute>());

                foreach (Type type in types)
                {
                    IEnumerable<(InputElement, int)> elements =
                        type.GetFields(flags)
                                .Select(static f => (f, a: f.GetCustomAttribute<InputElementAttribute>()))
                                .Where(static pair => pair.a != null)
                                .Select(static pair => (new InputElement(pair.f.Name, pair.f.FieldType, false), pair.a!.Index))
                                .Concat(
                                    type.GetProperties(flags)
                                        .Select(static p => (p, a: p.GetCustomAttribute<InputElementAttribute>()))
                                        .Where(static pair => pair.a != null)
                                        .Select(static pair => (new InputElement(pair.p.Name, pair.p.PropertyType, true), pair.a!.Index))
                                ).OrderBy(static pair => pair.Index);
                    {
                        int expected = 0;
                        foreach ((_, int index) in elements)
                        {
                            if (index != expected++)
                                throw new InvalidOperationException($"{type.Name} invalid element indexes");
                        }
                        if (expected == 0)
                            throw new InvalidOperationException($"{type.Name} has no valid input elements.");
                    }

                    elementMap[type] = [.. elements.Select(static pair => pair.Item1)];
                }

                _initialized = true;
            }
        }

        public string ReadLine()
        {
            return Stream.ReadLine() ?? string.Empty;
        }



        public string[] GetTokens()
        {
            return [.. Tokens];
        }

        public string? GetToken()
        {
            string? token;
            while (!Tokens.TryDequeue(out token))
            {
                if (!PopulateTokens())
                    return null;
            }
            return token;
        }

        private bool PopulateTokens()
        {
            if (!CanPopulate) return false;

            string input = ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            Tokens.Enqueue(StringUtils.Split(input));
            return true;
        }

        public object? Read(Type type)
        {
            lock (_instanceLock)
            {
                return ReadUnsafe(type);
            }
        }

        public bool? TryRead(Type type, [NotNullWhen(true)] out object? value)
        {
            value = Read(type);
            return value is not null;
        }

        public T? Read<T>()
            where T : notnull, new()
        {
            return (T?)Read(typeof(T));
        }

        public bool TryRead<T>([NotNullWhen(true)] out T? value)
            where T : notnull, new()
        {
            value = Read<T>();
            return value is not null;
        }

        internal object? ReadUnsafe(Type type)
        {
            if (elementMap.ContainsKey(type))
                return ReadComplex(type);
            return ReadSimple(type);
        }

        internal object? ReadSimple(Type type)
        {
            string? token = GetToken();
            if (token == null)
                return GetDefault(type);

            if (TypeParser.TryParse(token, type, out object? value))
                return value;

            return GetDefault(type);
        }


        internal object? ReadComplex(Type type)
        {
            if (!elementMap.TryGetValue(type, out InputElement[]? elements))
                throw new InvalidOperationException($"Input map for {type.Name} not found");

            object? obj = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException
                ($"{type.Name} cannot be deserialized because it does not have a parameterless constructor.");

            InputElement? failedElement = null;

            foreach (InputElement element in elements)
            {
                Type memberType = Nullable.GetUnderlyingType(element.MemberType) ?? element.MemberType;

                object? value = ReadUnsafe(memberType);

                if (value == null)
                {
                    failedElement ??= element;
                    continue;
                }

                if (element.IsProperty)
                {
                    PropertyInfo? prop = type.GetProperty(element.Name,
                        BindingFlags.Instance | BindingFlags.Public);
                    if (prop?.CanWrite == true)
                        prop.SetValue(obj, value);
                }
                else
                {
                    FieldInfo? field = type.GetField(element.Name);

                    if (field == null)
                    {
                        failedElement ??= element;
                        continue;
                    }

                    field.SetValue(obj, value);
                }
            }

            if (failedElement != null)
                throw new InvalidOperationException($"Field `{failedElement.Name}` of type `{type.Name}` failed to parse.");

            return obj;
        }

        public void Close()
        {
            Stream.Close();
        }

        private static object? GetDefault(Type type)
        {
            if (!type.IsValueType)
                return null;

            if (Nullable.GetUnderlyingType(type) != null)
                return null;

            return Activator.CreateInstance(type);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && !_skipDispose)
            {
                Stream?.Dispose();
            }

            _disposed = true;
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
