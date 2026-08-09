using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataLibrary
{
    public class BaseRecord
    {
        [JsonPropertyName("fields")]
        [JsonInclude]
        public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        public object? GetField(string fieldName)
        {
            if (!Fields.ContainsKey(fieldName))
            {
                return null;
            }

            return Fields[fieldName];
        }

        public JsonElement? GetFieldAsJson(string fieldName)
        {
            return GetField(fieldName) as JsonElement?;
        }

        public T? GetField<T>(string fieldName)
        {
            JsonElement? fieldAsJson = GetFieldAsJson(fieldName);
            if (fieldAsJson.HasValue)
            {
                JsonElement valueOrDefault = fieldAsJson.GetValueOrDefault();
                object obj = ParsePrimitiveValue(valueOrDefault, typeof(T));
                if (typeof(T) == typeof(DateTimeOffset))
                {
                    obj = valueOrDefault.GetDateTimeOffset();
                }

                return (T)obj;
            }

            return default(T);
        }

        public TEnumerable? GetField<TEnumerable, T>(string fieldName) where TEnumerable : class, IEnumerable<T>
        {
            JsonElement? fieldAsJson = GetFieldAsJson(fieldName);
            if (!fieldAsJson.HasValue)
            {
                return null;
            }

            IEnumerable<T> enumerable = (from _ in fieldAsJson.GetValueOrDefault().EnumerateArray()
                                         select ParsePrimitiveValue(_, typeof(T))).Cast<T>();
            Type typeFromHandle = typeof(TEnumerable);
            if ((object)typeFromHandle != null)
            {
                if (typeFromHandle == typeof(T[]))
                {
                    return enumerable.ToArray() as TEnumerable;
                }

                Type type = typeFromHandle;
                if (type == typeof(IList<T>) || type == typeof(ICollection<T>))
                {
                    return enumerable.ToList() as TEnumerable;
                }

                if (typeFromHandle == typeof(IEnumerable<T>))
                {
                    return (TEnumerable)enumerable;
                }
            }

            throw new NotSupportedException("Unknown enumerable type '" + typeof(TEnumerable).Name + "'");
        }

        private static object? ParsePrimitiveValue(JsonElement element, Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
                {
                    throw new NotSupportedException("The only generic type supported is Nullable<T>");
                }

                type = type.GenericTypeArguments.Single();
            }

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => element.GetBoolean(),
                TypeCode.SByte => element.GetSByte(),
                TypeCode.Byte => element.GetByte(),
                TypeCode.Int16 => element.GetInt16(),
                TypeCode.UInt16 => element.GetUInt16(),
                TypeCode.Int32 => element.GetInt32(),
                TypeCode.UInt32 => element.GetUInt32(),
                TypeCode.Int64 => element.GetInt64(),
                TypeCode.UInt64 => element.GetUInt64(),
                TypeCode.Single => element.GetDecimal(),
                TypeCode.Double => element.GetDouble(),
                TypeCode.Decimal => element.GetDecimal(),
                TypeCode.DateTime => element.GetDateTime(),
                TypeCode.String => element.GetString(),
                _ => null,
            };
        }
    }
}
