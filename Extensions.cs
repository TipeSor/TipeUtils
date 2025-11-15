using System.Collections;
using System.Reflection;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Extensions
    {
        public static bool IsEmpty(this ICollection source)
        {
            return source.Count == 0;
        }

        public static bool Implements<TInterface>(this Type type)
            where TInterface : class
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException($"{typeof(TInterface).Name} must be an interface type");

            return type.Implements(typeof(TInterface));
        }

        public static bool Implements(this Type type, Type genericType)
        {
            return Reflection.ImplementsInterface(type, genericType);
        }

        public static bool HasAttribute<T>(this MemberInfo element) where T : Attribute
        {
            return Attribute.IsDefined(element, typeof(T));
        }

        public static object? Invoke<T>(
            this T source,
            string name,
            Type[] parameterTypes,
            ref object?[] args)
        {
            object? target = source is Type ? null : source;
            Type sourceType = source is Type t ? t : typeof(T);

            return Reflection.Invoke(target, sourceType, name, parameterTypes, ref args);
        }

        public static bool TryInvoke<T, TResult>(
            this T source,
            string name,
            Type[] parameterTypes,
            out TResult? result,
            ref object?[] args)
        {
            try
            {
                object? returnValue = source.Invoke(name, parameterTypes, ref args);

                if (returnValue is TResult castResult)
                {
                    result = castResult;
                    return true;
                }

                result = default;
                return returnValue == null && default(TResult) == null;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
