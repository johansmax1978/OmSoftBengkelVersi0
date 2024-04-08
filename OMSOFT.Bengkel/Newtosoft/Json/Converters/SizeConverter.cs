#if NETFX && WIN32 && JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents the default <see cref="JsonConverter"/> instance for the <see cref="Size"/> data type.
    /// </summary>
    public partial class SizeConverter : JsonConverter<Size>
    {
        /// <summary>
        /// Try read the <see cref="Size"/> data from readed JSON token at current <paramref name="reader"/> position.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that being used to parse the <see cref="Size"/> at current position.</param>
        /// <param name="result">Retrieve <see cref="Size"/> value from readed JSON token, or <see cref="Size.Empty"/> on fail.</param>
        /// <returns><see langword="true"/> if the <see cref="Size"/> is successfuly parsed from JSON token; otherwise, <see langword="false"/>.</returns>
        public static Boolean TryRead(JsonReader reader, out Size result)
        {
            if (reader is null)
            {
                result = Size.Empty;
                return false;
            }
            else if ((reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.PropertyName) && !reader.Read())
            {
                result = Size.Empty;
                return false;
            }
            else
            {
                List<Int32> integers = null;
                switch (reader.TokenType)
                {
                    case JsonToken.StartArray:
                        integers = new List<Int32>(2);
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            if (reader.TokenType == JsonToken.Integer && integers.Count < 2)
                            {
                                integers.Add(Convert.ToInt32(reader.Value, CultureInfo.CurrentCulture));
                            }
                        }
                        if (integers.Count == 2)
                        {
                            result = new Size(integers[0], integers[1]);
                            return true;
                        }
                        result = Size.Empty;
                        return false;
                    case JsonToken.StartObject:
                        integers = new List<Int32>(2);
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                var property = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                                if (reader.Read())
                                {
                                    Int32 value;
                                    if (reader.TokenType == JsonToken.Integer)
                                    {
                                        value = Convert.ToInt32(reader.Value, CultureInfo.CurrentCulture);
                                    }
                                    else if (reader.TokenType == JsonToken.Float)
                                    {
                                        value = unchecked((Int32)Math.Round(Convert.ToDouble(reader.Value, CultureInfo.CurrentCulture), 0, MidpointRounding.AwayFromZero));
                                    }
                                    else if (reader.TokenType == JsonToken.Null)
                                    {
                                        value = 0;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    switch (property.ToLower())
                                    {
                                        case "width":
                                        case "x":
                                        case "w":
                                            integers[0] = value;
                                            break;
                                        case "height":
                                        case "y":
                                        case "h":
                                            if (integers.Count == 0) integers.Add(0);
                                            integers[1] = value;
                                            break;
                                    }
                                }
                            }
                        }
                        if (integers.Count == 2)
                        {
                            result = new Size(integers[0], integers[1]);
                            return true;
                        }
                        result = Size.Empty;
                        return false;
                    case JsonToken.Integer:
                        var int64 = Convert.ToInt64(reader.Value, CultureInfo.CurrentCulture);
                        unsafe
                        {
                            var intptr = (Int32*)(&int64);
                            result = new Size(intptr[0], intptr[1]);
                        }
                        return true;
                    case JsonToken.String:
                        var content = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                        if (content is null || content.Length == 0 || (content = content.Trim()).Length == 0)
                        {
                            result = Size.Empty;
                            return false;
                        }
                        if (Regex.Match(content, @"(?<w>\d+\.\d+|\d+)[\;|\:|\,]\s{0,}(?<h>\d+\.\d+|\d+)", RegexOptions.Compiled) is Match match && match.Success)
                        {
                            if (Double.TryParse(match.Groups["w"].Value, NumberStyles.Float, CultureInfo.CurrentCulture, out var w1) && Double.TryParse(match.Groups["h"].Value, NumberStyles.Float, CultureInfo.CurrentCulture, out var h1))
                            {
                                unchecked
                                {
                                    result = new Size((Int32)Math.Round(w1, 0, MidpointRounding.AwayFromZero), (Int32)Math.Round(h1, 0, MidpointRounding.AwayFromZero));
                                }
                                return true;
                            }
                            else if (Int32.TryParse(match.Groups["w"].Value, NumberStyles.Any, CultureInfo.CurrentCulture, out var w2) && Int32.TryParse(match.Groups["h"].Value, NumberStyles.Any, CultureInfo.CurrentCulture, out var h2))
                            {
                                result = new Size(w2, h2);
                                return true;
                            }
                        }
                        goto default;
                    default:
                        result = Size.Empty;
                        return false;
                }
            }

        }

        /// <summary>
        /// Try write the given <see cref="Size"/> value as JSON element or property value using the given <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> that should be used to write the given <see cref="Size"/> at current writing position.</param>
        /// <param name="value">The <see cref="Size"/> that should writen as JSON element or propert value using the given <see cref="JsonWriter"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="Size"/> is successfuly writen as JSON token; otherwise, <see langword="false"/>.</returns>
        public static Boolean TryWrite(JsonWriter writer, Size value)
        {
            if (writer is not null)
            {
                writer.WriteValue($"{value.Width};{value.Height}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="SizeConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly SizeConverter Default = new SizeConverter();

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeConverter"/> class.
        /// </summary>
        public SizeConverter() { }

        /// <inheritdoc/>
        public override Boolean CanRead => true;

        /// <inheritdoc/>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        /// <exception cref="JsonException">Thrown if the <see cref="JsonReader.Value"/> that being readed in the given <paramref name="reader"/> is not represents the correct <see cref="Size"/> format.</exception>
        public override Size ReadJson(JsonReader reader, Type objectType, Size existingValue, Boolean hasExistingValue, JsonSerializer serializer)
            => TryRead(reader, out var size) ? size : throw new JsonException($"The current JSON data at {reader.Path} is not represents the correct Size (width;height) format.");

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, Size value, JsonSerializer serializer)
            => _ = TryWrite(writer, value);

    }
}
#endif