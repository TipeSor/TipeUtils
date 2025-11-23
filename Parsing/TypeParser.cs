using System.ComponentModel;
using System.Globalization;

namespace TipeUtils.Parsing
{
    public static class TypeParser
    {
        public static Result<object> Parse(string token, Type type)
        {
            try
            {
                Type? _type;
                if ((_type = Nullable.GetUnderlyingType(type)) != null)
                    type = _type;

                if (type == typeof(string))
                    return Result<object>.Ok(token);

                if (string.IsNullOrWhiteSpace(token))
                    return Result<object>.Error("Token cannot be null or whitespace for non-string types");


                if (type.IsEnum)
                {
                    object value = Enum.Parse(type, token, ignoreCase: true);
                    return Result<object>.Ok(value);
                }

                if (typeof(IConvertible).IsAssignableFrom(type))
                {
                    try
                    {
                        object value = Convert.ChangeType(token, type, CultureInfo.InvariantCulture);
                        return Result<object>.Ok(value);
                    }
                    catch (FormatException) { }
                }

                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        object value = converter.ConvertFromString(token)!;
                        return Result<object>.Ok(value);
                    }
                    catch (Exception ex) when (ex is FormatException or NotSupportedException) { }
                }

                throw new ArgumentException($"Failed to parse '{token}' to type '{type.FullName}'.");
            }
            catch (Exception ex)
            {
                return Result<object>.Error(ex);
            }
        }

        public static Result<T> Parse<T>(string token)
            where T : notnull
        {
            return Parse(token, typeof(T));
        }
    }
}
