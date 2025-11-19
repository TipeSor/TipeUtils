using System.Reflection;

namespace TipeUtils.Reflection
{
    public static class ReflectionUtils
    {
        public static object? GenericCreator(Type type, Type genericType)
        {
            try
            {
                if (!genericType.IsGenericTypeDefinition) return default;
                Type typedType = genericType.MakeGenericType(type);
                return Activator.CreateInstance(typedType);
            }
            catch { return default; }
        }

        internal static bool ImplementsInterface(Type type, Type interfaceType)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.GetInterfaces().Any(i => i.IsGenericType &&
            i.GetGenericTypeDefinition() == interfaceType))
                return true;

            return false;
        }

        internal static object? Invoke(
            object? instance,
            Type instanceType,
            string methodName,
            Type[] parameterTypes,
            object?[] args)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                      BindingFlags.Instance | BindingFlags.Static;

            IEnumerable<MethodInfo> methods =
                    instanceType.GetMethods(flags)
                                .Where(m => m.Name == methodName &&
                                            m.GetParameters().Length == parameterTypes.Length);

            foreach (MethodInfo? method in methods.Where(m => m.IsGenericMethod))
            {
                try
                {
                    ParameterInfo[] methodParams = method.GetParameters();
                    List<Type> genericTypes = [];

                    for (int i = 0; i < methodParams.Length; i++)
                    {
                        Type paramType = methodParams[i].ParameterType;
                        Type argType = parameterTypes[i];

                        if (paramType.IsGenericParameter)
                        {
                            genericTypes.Add(argType);
                        }
                        else if (paramType.IsByRef && paramType.GetElementType()!.IsGenericParameter)
                        {
                            genericTypes.Add(argType.GetElementType() ?? argType);
                        }
                    }

                    if (genericTypes.Count > 0)
                    {
                        MethodInfo constructedMethod = method.MakeGenericMethod([.. genericTypes]);
                        return constructedMethod.Invoke(instance, args);
                    }
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to invoke {methodName} on {instanceType}", ex);
                }
            }

            MethodInfo[] nonGenericMethods = [.. methods
                .Where(m => !m.IsGenericMethodDefinition)
                .GroupBy(m => m.MetadataToken)
                .Select(g => g.First())];

            if (nonGenericMethods.Length == 0)
                return null;

            MethodInfo? selectedMethod = Type.DefaultBinder.SelectMethod(
                flags | BindingFlags.InvokeMethod,
                nonGenericMethods,
                parameterTypes,
                null) as MethodInfo;

            return selectedMethod?.Invoke(instance, args);
        }
    }
}
