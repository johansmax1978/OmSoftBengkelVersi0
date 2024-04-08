#if JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Represents the default <see cref="JsonConverter"/> instance for the <see cref="StringBuilder"/> class.
    /// </summary>
    public sealed partial class StringBuilderConverter : JsonConverter<StringBuilder>
    {
        /// <summary>
        /// Represents the default singleton instance of the <see cref="StringBuilderConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly StringBuilderConverter Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StringBuilderConverter"/> class.
        /// </summary>
        public StringBuilderConverter() { }

        /// <inheritdoc/>
        public override Boolean CanRead => true;

        /// <inheritdoc/>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        public override StringBuilder ReadJson(JsonReader reader, Type objectType, StringBuilder existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                if (reader.TokenType == JsonToken.None && !reader.Read())
                {
                    return null;
                }
                var useExisting = hasExistingValue && serializer is not null && serializer.ObjectCreationHandling != ObjectCreationHandling.Replace;
                StringBuilder buffer;
                switch (reader.TokenType)
                {
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        return null;
                    case JsonToken.PropertyName:
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Boolean:
                    case JsonToken.Date:
                        buffer = useExisting ? existingValue : new StringBuilder(8192);
                        buffer.Append(Convert.ToString(reader.Value, CultureInfo.CurrentCulture));
                        return buffer;
                    case JsonToken.Bytes:
                        buffer = useExisting ? existingValue : new StringBuilder(8192);
                        if (reader.Value is Byte[] bytes)
                            _ = buffer.Append(Convert.ToBase64String(bytes, Base64FormattingOptions.None));
                        else
                            _ = buffer.Append(reader.Value);
                        return buffer;
                    default:
                        throw new JsonException($"Failed to parse {nameof(StringBuilder)} object from JSON token {reader.TokenType}, expected to be a String token.");

                }
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, StringBuilder value, JsonSerializer serializer)
        {
            if (value is null) writer.WriteNull(); else writer.WriteValue(value.ToString());
        }
    }
}
#endif