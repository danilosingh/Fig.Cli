using Fig.Cli.Extensions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Fig.Cli.Helpers
{
    public class TypeHelper
    {
        public static T Convert<T>(object value, CultureInfo cultureInfo = null)
        {
            var toType = typeof(T);

            if (value == null || value is DBNull)
            {
                return default;
            }

            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }

            if (value is string @string)
            {
                if (toType == typeof(string[]))
                {
                    return value == null ? default(T) : (T)(object)@string.Split("|");
                }
                if (toType == typeof(Guid?))
                {
                    return Convert<T>(value == null ? default(Guid?) : new Guid(System.Convert.ToString(value, cultureInfo)), cultureInfo);
                }
                if (toType == typeof(Guid))
                {
                    return Convert<T>(new Guid(System.Convert.ToString(value, cultureInfo)), cultureInfo);
                }
                if (@string == string.Empty && toType != typeof(string))
                {
                    return Convert<T>(null, cultureInfo);
                }
            }
            else
            {
                if (typeof(T) == typeof(string))
                {
                    return Convert<T>(System.Convert.ToString(value, cultureInfo), cultureInfo);
                }
            }

            if (toType.IsGenericType &&
                toType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                toType = Nullable.GetUnderlyingType(toType); ;
            }

            if (toType.IsEnum && value != null)
            {
                return (T)Enum.Parse(toType, value.ToString());
            }
            else if (toType == typeof(TimeSpan))
            {
                var converter = TypeDescriptor.GetConverter(typeof(TimeSpan));
                return (T)converter.ConvertFrom(value);
            }

            bool canConvert = toType is IConvertible || (toType.IsValueType && !toType.IsEnum);

            if (canConvert)
            {
                return (T)System.Convert.ChangeType(value, toType, cultureInfo);
            }
            return (T)value;
        }

        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
