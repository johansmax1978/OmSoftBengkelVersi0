#if NETFX && WIN32 && JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Represents the default <see cref="JsonConverter"/> instance for the <see cref="Color"/> data type.
    /// </summary>
    public partial class ColorConverter : JsonConverter<Color>
    {
        /// <summary>
        /// Try to read the <see cref="Color"/> value by parsing the information that being readed at current JSON <paramref name="reader"/> position.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that being used to read JSON, where the <see cref="Color"/> will be parsed at current position.</param>
        /// <param name="color">Retrieve the <see cref="Color"/> when succeeds to parse the data from JSON, otherwise, retrieve <see cref="Color.Empty"/>.</param>
        /// <returns>A <see cref="Boolean"/> value, where <see langword="true"/> if the <see cref="Color"/> was successfuly parsed from JSON token; otherwise, <see langword="false"/> on failed.</returns>
        public static Boolean TryRead(JsonReader reader, out Color color) => TryRead(reader, out color, Color.Empty);

        /// <summary>
        /// Try to read the <see cref="Color"/> value by parsing the information that being readed at current JSON <paramref name="reader"/> position.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that being used to read JSON, where the <see cref="Color"/> will be parsed at current position.</param>
        /// <param name="color">Retrieve the <see cref="Color"/> when succeeds to parse the data from JSON, otherwise, retrieve <paramref name="fallback"/> color.</param>
        /// <param name="fallback">The default <see cref="Color"/> that should be used to set the output <paramref name="color"/> if parsing JSON token data was failed.</param>
        /// <returns>A <see cref="Boolean"/> value, where <see langword="true"/> if the <see cref="Color"/> was successfuly parsed from JSON token; otherwise, <see langword="false"/> on failed.</returns>
        public static Boolean TryRead(JsonReader reader, out Color color, Color fallback)
        {
            if (reader is null)
            {
                color = fallback;
                return false;
            }
            if ((reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.PropertyName) && !reader.Read())
            {
                color = fallback;
                return false;
            }
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                case JsonToken.Undefined:
                    color = Color.Empty;
                    return true;
                case JsonToken.Integer:
                    color = Color.FromArgb(Convert.ToInt32(reader.Value, CultureInfo.CurrentCulture));
                    return true;
                case JsonToken.String:
                    var content = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                    if (content is null || content.Length == 0 || (content = content.Trim()).Length == 0)
                    {
                        color = Color.Empty;
                        return true;
                    }
                    else
                    {
                        var hexnumber = 0;
                        var hex1 = content[0] == '#' && content.Length > 1;
                        var hex2 = !hex1 && content.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                        var hex3 = !hex1 && !hex2 && Int32.TryParse(content, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexnumber);
                        if (!hex3)
                        {
                            hex3 = hex1 ? Int32.TryParse(content.Substring(1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexnumber)
                                    : hex2 ? Int32.TryParse(content.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexnumber)
                                           : false;
                        }
                        if (hex3)
                        {
                            color = Color.FromArgb(hexnumber);
                            return true;
                        }
                        if (ColorMaps.TryGetValue(content, out color)) return true;
                        if (Enum.TryParse<KnownColor>(content, true, out var known))
                        {
                            color = Color.FromKnownColor(known);
                            return true;
                        }
                        switch (content.ToLower())
                        {
                            case "empty":
                            case "none":
                            case "null":
                            case "nothing":
                            case "default":
                                color = Color.Empty;
                                return true;
                            case "transparent":
                            case "alpha":
                                color = Color.Transparent;
                                return true;
                            default:
                                color = fallback;
                                return true;
                        }
                    }
                default:
                    color = fallback;
                    return false;
            }
        }

        /// <summary>
        /// Try to write the given <see cref="Color"/> into the underlying JSON document through the given JSON <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> that should be used to write the given <see cref="Color"/> value at current position.</param>
        /// <param name="value">Specify the desired <see cref="Color"/> value that should writen as JSON using the given <see cref="JsonWriter"/>.</param>
        /// <returns><see langword="true"/> if the given <see cref="Color"/> <paramref name="value"/> is succeeds writen using the given <see cref="JsonWriter"/>; otherwise, <see langword="false"/>.</returns>
        public static Boolean TryWrite(JsonWriter writer, Color value)
        {
            if (writer is not null)
            {
                writer.WriteValue($"#{value.ToArgb():x8}");
                return true;
            }
            return false;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Dictionary<String, Color> ColorMaps;

        static ColorConverter()
        {
            var maps = ColorMaps = new Dictionary<String, Color>(1024, StringComparer.OrdinalIgnoreCase);
            foreach (var property in typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (property.PropertyType == typeof(Color))
                {
                    var value = (Color)property.GetValue(null);
                    maps[property.Name] = value;
                    maps[$"#{value.ToArgb():x8}"] = value;
                    if (!maps.ContainsKey(value.Name)) maps[value.Name] = value;
                }
            }
            Default = new ColorConverter();
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="ColorConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly ColorConverter Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorConverter"/> class.
        /// </summary>
        public ColorConverter() { }

        /// <inheritdoc/>
        public override Boolean CanRead => true;

        /// <inheritdoc/>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        /// <exception cref="JsonException">Thrown if the <see cref="JsonReader.Value"/> that being readed is not represents the valid <see cref="Color"/> information.</exception>
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, Boolean hasExistingValue, JsonSerializer serializer)
            => TryRead(reader, out var color, existingValue) ? color : throw new JsonException($"The JSON data at {reader.Path} is not represents a valid color to parse.");

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
            => _ = TryWrite(writer, value);
    }
}
#endif