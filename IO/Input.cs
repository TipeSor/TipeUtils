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

        private static readonly Dictionary<Type, Func<Input, Result<object>>> factories = [];
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
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                ProcessAssembly(asm);

            _initialized = true;
        }

        public static void Register(Assembly? asm = null)
        {
            asm ??= Assembly.GetCallingAssembly();
            lock (_initLock)
            {
                ProcessAssembly(asm);
            }
        }

        private static void ProcessAssembly(Assembly assembly)
        {
            IEnumerable<(Type, ComplexObjectAttribute)> types = assembly.GetTypes()
                .Select(static t => (type: t, atr: t.GetCustomAttribute<ComplexObjectAttribute>()))
                .Where(static obj => obj.atr != null)
                .Select(static obj => (obj.type, obj.atr!));

            foreach ((Type type, ComplexObjectAttribute atr) in types)
            {
                switch (atr.FactoryMethodType)
                {
                    case FactoryMethodType.FieldInitializer:
                        FieldFactory(type);
                        break;
                    case FactoryMethodType.Constructor:
                        CtorFactory(type);
                        break;
                    case FactoryMethodType.CustomMethod:
                        CustomFactory(type);
                        break;
                    case FactoryMethodType.AutoSelect:
                        ProccessAuto(type);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ProccessAuto(Type type)
        {
            if (type.GetMethods().Any(static m => m.HasAttribute<FactoryMethodAttribute>()))
            {
                CustomFactory(type);
                return;
            }

            if (type.GetConstructor(Type.EmptyTypes) != null &&
                (type.GetFields().Any(static f => f.HasAttribute<InputParameterAttribute>()) ||
                 type.GetProperties().Any(static p => p.HasAttribute<InputParameterAttribute>())))
            {
                FieldFactory(type);
                return;
            }

            CtorFactory(type);


        }

        private static void FieldFactory(Type type)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            List<(InputParameterInfo element, int index)> elements = [];

            foreach (FieldInfo f in type.GetFields(flags))
            {
                if (f.GetCustomAttribute<InputParameterAttribute>() is { } a)
                    elements.Add((new InputParameterInfo(f.Name, f.FieldType, false), a.Index));
            }

            foreach (PropertyInfo p in type.GetProperties(flags))
            {
                if (p.GetCustomAttribute<InputParameterAttribute>() is { } a)
                    elements.Add((new InputParameterInfo(p.Name, p.PropertyType, true), a.Index));
            }

            elements.Sort(static (a, b) => a.index.CompareTo(b.index));

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].index != i)
                    throw new InvalidOperationException($"{type.Name} invalid element indexes");
            }

            if (elements.Count == 0)
                throw new InvalidOperationException($"{type.Name} has no valid input elements.");

            InputParameterInfo[] infos = [.. elements.Select(static e => e.element)];

            factories[type] = input =>
            {
                object? obj = Activator.CreateInstance(type);

                InputParameterInfo? failedElement = null;

                for (int i = 0; i < infos.Length; i++)
                {
                    InputParameterInfo element = infos[i];
                    Type memberType = Nullable.GetUnderlyingType(element.MemberType) ?? element.MemberType;

                    Result<object> result = input.ReadUnsafe(memberType);

                    if (result.IsError)
                    {
                        failedElement = element;
                        for (int j = i + 1; j < infos.Length; j++)
                            input.GetToken();
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

                return Result<object>.Ok(obj!);
            };
        }

        private static void CtorFactory(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Length == 0)
                return;

            ConstructorInfo constructor =
                constructors.FirstOrDefault(static c => Attribute.IsDefined(c, typeof(InputConstructorAttribute))) ??
                constructors.First();

            IEnumerable<Type> argTypes = constructor.GetParameters().Select(p => p.ParameterType);

            factories[type] = input =>
            {
                List<object?> args = [];
                foreach (Type type in argTypes)
                {
                    Result<object> arg = input.Read(type);
                    args.Add(arg.Value);
                }

                object? obj = constructor.Invoke([.. args]);

                return obj == null
                    ? Result<object>.Error("TODO: Input.CtorFactory")
                    : Result<object>.Ok(obj);
            };
        }

        private static void CustomFactory(Type type)
        {
            IEnumerable<(MethodInfo info, FactoryMethodAttribute)> data = type.GetMethods()
                           .Select(static m => (info: m, atr: m.GetCustomAttribute<FactoryMethodAttribute>()))
                           .Where(static obj => obj.atr != null)
                           .Select(static obj => (obj.info, obj.atr!));

            foreach ((MethodInfo info, FactoryMethodAttribute atr) in data)
            {
                if (!info.IsStatic ||
                    info.ReturnType != typeof(Result<object>) ||
                    info.GetParameters().Length != 1 ||
                    info.GetParameters()[0].ParameterType != typeof(Input))
                    return;

                factories[atr.OutputType] = input =>
                {
                    object? obj = info.Invoke(null, [input]);
                    return obj is Result<object> result
                        ? result
                        : Result<object>.Error("TODO: Input.CustomFactory");
                };
            }
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
                return factories.ContainsKey(type)
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
                return factories[type](this);
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
    public class ComplexObjectAttribute(FactoryMethodType factoryType = FactoryMethodType.AutoSelect) : Attribute
    {
        public FactoryMethodType FactoryMethodType { get; } = factoryType;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InputParameterAttribute(int index) : Attribute
    {
        public int Index { get; } = index;
    }

    internal record InputParameterInfo(string Name, Type MemberType, bool IsProperty);

    [AttributeUsage(AttributeTargets.Constructor)]
    public class InputConstructorAttribute() : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class FactoryMethodAttribute(Type outputType) : Attribute
    {
        public Type OutputType { get; } = outputType;
    }

    public enum FactoryMethodType
    {
        FieldInitializer,
        Constructor,
        CustomMethod,
        AutoSelect
    }
}
