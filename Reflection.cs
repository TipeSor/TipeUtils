using System.Reflection;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Reflection
    {
        public static object? GenericCreator(Type type, Type genericType)
        {
            try
            {
                if (!genericType.IsGenericType) return default;
                Type typedType = genericType.MakeGenericType(type);
                return Activator.CreateInstance(typedType);
            }
            catch { return default; }
        }

        public static bool ImplementsGeneric(Type type, Type genericType)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.GetInterfaces().Any(i => i.IsGenericType &&
            i.GetGenericTypeDefinition() == genericType))
                return true;

            return false;
        }

        public static object? Invoke(
            object? instance,
            Type instanceType,
            string methodName,
            Type[] parameterTypes,
            ref object?[] args
        )
        {
            try
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Instance | BindingFlags.Static;


                MethodInfo[] methods = [.. instanceType.GetMethods(flags)
                                                   .Where(m => m.Name == methodName &&
                                                               m.GetParameters().Length == parameterTypes.Length)];

                foreach (MethodInfo m in methods)
                {
                    if (m.IsGenericMethod)
                    {
                        try { return m.MakeGenericMethod(parameterTypes).Invoke(instance, args); }
                        catch { }
                    }
                }

                MethodBase? methodBase = Type.DefaultBinder.SelectMethod(
                    flags,
                    methods,
                    parameterTypes,
                    null);

                if (methodBase == null)
                    return null;

                if (methodBase is not MethodInfo method)
                    return null;

                return method.Invoke(instance, args);
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }
            catch (MissingMethodException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
