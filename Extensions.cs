
#pragma warning disable IDE0011, IDE0046, IDE0058
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TipeUtils
{
    public static class Extensions
    {
        public static bool IsEmpty(this ICollection source)
        {
            return source.Count == 0;
        }

        public static bool Implements(this Type source, Type genericType)
        {
            return Reflection.ImplementsGeneric(source, genericType);
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
            [NotNullWhen(true)] out TResult? value,
            ref object?[] args)
            where T : notnull
        {
            if (source.Invoke(name, parameterTypes, ref args) is TResult result)
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }
    }
}
