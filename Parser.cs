using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Parser
    {
        public static T FromString<T>(string token)
        {
            Type type = typeof(T);

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            if (type.IsEnum)
                return (T)Enum.Parse(type, token, ignoreCase: true);

            if (typeof(IConvertible).IsAssignableFrom(type))
                return (T)Convert.ChangeType(token, type, CultureInfo.InvariantCulture);

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
                return (T)converter.ConvertFromString(token)!;

            MethodInfo? parseMethod = type.GetMethod("Parse", [typeof(string)]);
            if (parseMethod != null)
                return (T)parseMethod.Invoke(null, [token])!;

            throw new ArgumentException("Failed to parse.");
        }

        public static bool TryFromString<T>(string token, [NotNullWhen(true)] out T? value)
             where T : notnull
        {
            try { value = FromString<T>(token); return true; }
            catch { value = default; return false; }
        }
    }
}
