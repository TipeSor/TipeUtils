using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Parser
    {
        public static object? FromString(string token, Type type)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            {
                type = underlyingType;
                if (string.IsNullOrWhiteSpace(token))
                    return default!;
            }

            if (type.IsEnum)
                return Enum.Parse(type, token, ignoreCase: true);

            if (typeof(IConvertible).IsAssignableFrom(type))
                return Convert.ChangeType(token, type, CultureInfo.InvariantCulture);

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFromString(token)!;

            MethodInfo? parseMethod = type.GetMethod("Parse", [typeof(string)]);
            if (parseMethod != null)
                return parseMethod.Invoke(null, [token])!;

            throw new InvalidOperationException($"Failed to parse {token} to type {type.Name}");
        }

        public static bool TryFromString(string token, Type type, [NotNullWhen(true)] out object? value)
        {
            try { value = FromString(token, type)!; return true; }
            catch (Exception) { value = null; return false; }
        }

        public static T? FromString<T>(string token)
        {
            return (T?)FromString(token, typeof(T));
        }

        public static bool TryFromString<T>(string token, [NotNullWhen(true)] out T? value)
        {
            bool res = TryFromString(token, typeof(T), out object? temp);
            value = (T?)temp;
            return res;
        }
    }
}
