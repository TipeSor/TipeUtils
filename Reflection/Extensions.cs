using System.Reflection;

namespace TipeUtils.Reflection
{
    public static class Extensions
    {
        public static bool Implements<TInterface>(this Type type)
            where TInterface : class
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException($"{typeof(TInterface).Name} must be an interface type");

            return type.Implements(typeof(TInterface));
        }

        public static bool Implements(this Type type, Type genericType)
        {
            return ReflectionUtils.ImplementsInterface(type, genericType);
        }

        public static bool HasAttribute<T>(this MemberInfo element) where T : Attribute
        {
            return Attribute.IsDefined(element, typeof(T));
        }

        public static Result<TResult, string> Invoke<T, TResult>(
            this T source,
            string name,
            Type[] parameterTypes,
            object?[] args)
        {
            object? target = source is Type ? null : source;
            Type sourceType = source is Type t ? t : typeof(T);

            try
            {
                object? value = ReflectionUtils.Invoke(target, sourceType, name, parameterTypes, args);
                if (value is TResult typed)
                    return Result<TResult, string>.Ok(typed);
                return Result<TResult, string>.Err("Faied to invoke.");
            }
            catch (Exception ex)
            {
                return Result<TResult, string>.Err(ex.Message);
            }
        }
    }
}
