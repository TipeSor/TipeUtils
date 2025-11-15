using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Parser
    {
        public static object Parse(string token, Type type)
        {
            Type? _type;
            if ((_type = Nullable.GetUnderlyingType(type)) != null)
                type = _type;

            if (type == typeof(string))
                return token;

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or whitespace for non-string types");


            if (type.IsEnum)
                return Enum.Parse(type, token, ignoreCase: true);

            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                try { return Convert.ChangeType(token, type, CultureInfo.InvariantCulture); }
                catch (FormatException) { }
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try { return converter.ConvertFromString(token)!; }
                catch (Exception ex) when (ex is FormatException or NotSupportedException) { }
            }

            throw new ArgumentException($"Failed to parse '{token}' to type '{type.FullName}'.");
        }

        public static bool TryParse(string token, Type type, [NotNullWhen(true)] out object? value)
        {
            try { value = Parse(token, type); return true; }
            catch { value = default; return false; }
        }

        public static T Parse<T>(string token)
        {
            return (T)Parse(token, typeof(T));
        }

        public static bool TryParse<T>(string token, [NotNullWhen(true)] out T? value)
        {
            try { value = Parse<T>(token)!; return true; }
            catch { value = default; return false; }
        }
    }
}
