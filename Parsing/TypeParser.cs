using System.ComponentModel;
using System.Globalization;

namespace TipeUtils.Parsing
{
    public static class TypeParser
    {
        public static Result<object, string> Parse(string token, Type type)
        {
            try
            {
                Type? _type;
                if ((_type = Nullable.GetUnderlyingType(type)) != null)
                    type = _type;

                if (type == typeof(string))
                    return Result<object, string>.Ok(token);

                if (string.IsNullOrWhiteSpace(token))
                    return Result<object, string>.Err("Token cannot be null or whitespace for non-string types");

                if (type.IsEnum)
                {
                    object value = Enum.Parse(type, token, ignoreCase: true);
                    return Result<object, string>.Ok(value);
                }

                if (typeof(IConvertible).IsAssignableFrom(type))
                {
                    try
                    {
                        object value = Convert.ChangeType(token, type, CultureInfo.InvariantCulture);
                        return Result<object, string>.Ok(value);
                    }
                    catch (FormatException) { }
                }

                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        object value = converter.ConvertFromString(token)!;
                        return Result<object, string>.Ok(value);
                    }
                    catch (Exception ex) when (ex is FormatException or NotSupportedException) { }
                }

                throw new ArgumentException($"Failed to parse '{token}' to type '{type.FullName}'.");
            }
            catch (Exception ex)
            {
                return Result<object, string>.Err(ex.Message);
            }
        }

        public static Result<T, string> Parse<T>(string token)
        {
            return Parse(token, typeof(T)).Cast<T>();
        }
    }
}
