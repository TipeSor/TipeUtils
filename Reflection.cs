using System.Reflection;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Reflection
    {
        public static object? GenericCreator(Type baseType, Type type)
        {
            if (!baseType.IsGenericType) return null;
            Type typedType = baseType.MakeGenericType(type);
            return Activator.CreateInstance(typedType);
        }

        public static object? GenericInvoker(
            Type targetType,
            object? instance,
            string methodName,
            Type[] genericTypes,
            params object[] args
        )
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance | BindingFlags.Static;

            MethodInfo? methodInfo = targetType.GetMethod(methodName, flags);

            if (methodInfo == null)
                throw new MissingMethodException(targetType.FullName, methodName);

            if (!methodInfo.IsGenericMethodDefinition)
                throw new InvalidOperationException($"{methodName} is not a generic method definition.");

            MethodInfo genericMethod = methodInfo.MakeGenericMethod(genericTypes);

            return genericMethod.Invoke(instance, args);
        }

    }
}
