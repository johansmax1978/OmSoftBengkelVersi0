#if NETFX && WIN32 && JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;

    /// <summary>
    /// Represents the default <see cref="JsonConverter"/> instance for the <see cref="Font"/> object type.
    /// </summary>
    [Serializable]
    public partial class FontConverter : JsonConverter<Font>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly System.Drawing.FontConverter DefaultConverter = new System.Drawing.FontConverter();

        private static Boolean ParseEnum<T>(Object source, out T result) where T : struct
        {
            if (source is null)
            {
                result = default;
                return true;
            }
            else
            {
                try
                {
                    var content = source is String textual ? textual : source is Int32 integer ? integer.ToString() : Convert.ToString(source, CultureInfo.CurrentCulture);
                    if (Int64.TryParse(content, NumberStyles.Any, CultureInfo.CurrentCulture, out var number))
                    {
                        result = (T)Enum.ToObject(typeof(T), number);
                        return true;
                    }
                    else if (Enum.TryParse<T>(content, true, out result))
                    {
                        return true;
                    }
                    result = default;
                    return false;
                }
                catch
                {
                    result = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Attempt read the <see cref="Font"/> value by parsing the information that being readed at current JSON <paramref name="reader"/> position.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that being used to read JSON, where the <see cref="Color"/> will be parsed at current position.</param>
        /// <returns>Retrieve the <see cref="Font"/> object when succeeds to parse the data from JSON, otherwise, throw <see cref="JsonException"/>.</returns>
        /// <exception cref="JsonException">The current readed JSON is not represents the valid <see cref="Font"/> object.</exception>
        public static Font ReadFont(JsonReader reader)
            => TryRead(reader, out var font, out var error) ? font : throw new JsonException(error);

        /// <summary>
        /// Try to read the <see cref="Font"/> value by parsing the information that being readed at current JSON <paramref name="reader"/> position.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that being used to read JSON, where the <see cref="Color"/> will be parsed at current position.</param>
        /// <param name="font">Retrieve the <see cref="Font"/> object when succeeds to parse the data from JSON, otherwise, retrieve <see langword="null"/> when failed.</param>
        /// <returns>A <see cref="Boolean"/> value, where <see langword="true"/> if the <see cref="Font"/> was successfuly parsed from JSON token; otherwise, <see langword="false"/> on failed.</returns>
        public static Boolean TryRead(JsonReader reader, out Font font)
            => TryRead(reader, out font, out _);

        /// <summary>
        /// Try to read the <see cref="Font"/> value by parsing the information that being readed at current JSON <paramref name="reader"/> position.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that being used to read JSON, where the <see cref="Color"/> will be parsed at current position.</param>
        /// <param name="font">Retrieve the <see cref="Font"/> object when succeeds to parse the data from JSON, otherwise, retrieve <see langword="null"/> when failed.</param>
        /// <param name="error">Retrieve <see langword="null"/> on succeeds, or retrieve a <see cref="String"/> as message of the error that being occured.</param>
        /// <returns>A <see cref="Boolean"/> value, where <see langword="true"/> if the <see cref="Font"/> was successfuly parsed from JSON token; otherwise, <see langword="false"/> on failed.</returns>
        public static Boolean TryRead(JsonReader reader, out Font font, out String error)
        {
            if (reader is null)
            {
                error = "The JSON reader to be used must not be null.";
                font = null;
                return false;
            }
            if ((reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.PropertyName) && !reader.Read())
            {
                error = "The given JSON reader is not yield any element or property value.";
                font = null;
                return false;
            }
            if (reader.TokenType == JsonToken.Null)
            {
                font = null;
                error = null;
                return true;
            }
            var path = reader.Path;
            if (reader.TokenType == JsonToken.String)
            {
                try
                {
                    font = (Font)DefaultConverter.ConvertFromString(Convert.ToString(reader.Value, CultureInfo.CurrentCulture));
                    error = null;
                    return true;
                }
                catch (Exception exception)
                {
                    error = $"[{path}] {exception.GetType().Name}: {exception.Message}";
                    font = null;
                    return false;
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                FontStyle? style = null;
                GraphicsUnit? unit = null;
                Byte? gdiCharSet = null;
                Single? emSize = null;
                Boolean? gdiVertical = null;
                String family = null;
                try
                {
                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            var property = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                            if (reader.Read() && property is not null)
                            {
                                switch (property.ToLower())
                                {
                                    case "family":
                                    case "fontfamily":
                                    case "font_family":
                                    case "name":
                                    case "fontname":
                                    case "font_name":
                                    case "font":
                                        family = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "emsize":
                                    case "em_size":
                                    case "size":
                                    case "fontsize":
                                    case "font_size":
                                        emSize = reader.Value is null ? null : Convert.ToSingle(reader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "unit":
                                    case "graphicsunit":
                                    case "sizeunit":
                                    case "graphics_unit":
                                    case "size_unit":
                                    case "fontunit":
                                    case "font_unit":
                                        if (ParseEnum<GraphicsUnit>(reader.Value, out var jsonUnit))
                                            unit = jsonUnit;
                                        break;
                                    case "style":
                                    case "styles":
                                    case "fontstyle":
                                    case "fontstyles":
                                    case "font_style":
                                    case "font_styles":
                                        if (ParseEnum<FontStyle>(reader.Value, out var jsonstyle))
                                            style = jsonstyle;
                                        break;
                                    case "gdicharset":
                                    case "gdi_char_set":
                                    case "gdi_charset":
                                    case "charset":
                                        try { gdiCharSet = Convert.ToByte(reader.Value, CultureInfo.CurrentCulture); }
                                        catch { gdiCharSet = null; }
                                        break;
                                    case "gdivertical":
                                    case "gdi_vertical":
                                    case "vertical":
                                    case "is_vertical":
                                        try { gdiVertical = Convert.ToBoolean(reader.Value, CultureInfo.CurrentCulture); }
                                        catch { gdiVertical = false; }
                                        break;

                                }
                            }
                        }
                    }
                    if (family is not null && family.Length != 0)
                    {
                        var sizeunit = unit.GetValueOrDefault(GraphicsUnit.Point);
                        if (!emSize.HasValue) emSize = sizeunit == GraphicsUnit.Pixel ? 12f : 8f;
                        if (gdiCharSet.HasValue)
                        {
                            if (gdiVertical.HasValue)
                            {
                                font = new Font(family, emSize.Value, style.GetValueOrDefault(FontStyle.Regular), sizeunit, gdiCharSet.Value, gdiVertical.Value);
                                error = null;
                                return true;
                            }
                            else
                            {
                                font = new Font(family, emSize.Value, style.GetValueOrDefault(FontStyle.Regular), sizeunit, gdiCharSet.Value);
                                error = null;
                                return true;
                            }
                        }
                        else
                        {
                            font = new Font(family, emSize.Value, style.GetValueOrDefault(FontStyle.Regular), sizeunit);
                            error = null;
                            return true;
                        }
                    }
                    error = $"[{path}] Font family is not found in the current readed JSON object.";
                    font = null;
                    return false;
                }
                catch (Exception exception)
                {
                    error = $"[{path}] {exception.GetType().Name}: {exception.Message}";
                    font = null;
                    return false;
                }
            }
            else
            {
                error = $"[{path}] Requires either JSON string or JSON object to parse as font.";
                font = null;
                return false;
            }
        }

        /// <summary>
        /// Try to write the given <see cref="Font"/> into the underlying JSON document through the given JSON <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> that should be used to write the given <see cref="Color"/> value at current position.</param>
        /// <param name="value">Specify the desired <see cref="Font"/> object that should writen as JSON using the given <see cref="JsonWriter"/>.</param>
        /// <returns><see langword="true"/> if the given <see cref="Color"/> <paramref name="value"/> is succeeds writen using the given <see cref="JsonWriter"/>; otherwise, <see langword="false"/>.</returns>
        public static Boolean TryWrite(JsonWriter writer, Font value)
        {
            if (writer is not null)
            {
                if (value is null)
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName("family");
                        writer.WriteValue(value.FontFamily?.ToString());
                        writer.WritePropertyName("size");
                        writer.WriteValue(value.Size);
                        writer.WritePropertyName("style");
                        writer.WriteValue(value.Style.ToString());
                        writer.WritePropertyName("unit");
                        writer.WriteValue(value.Unit.ToString());
                        writer.WriteValue("charset");
                        writer.WriteValue(value.GdiCharSet);
                        writer.WriteValue("vertical");
                        writer.WriteValue(value.GdiVerticalFont);
                    }
                    writer.WriteEndObject();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="FontConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly FontConverter Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FontConverter"/> class.
        /// </summary>
        public FontConverter() { }

        /// <inheritdoc/>
        public override Boolean CanRead => true;

        /// <inheritdoc/>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        public override Font ReadJson(JsonReader reader, Type objectType, Font existingValue, Boolean hasExistingValue, JsonSerializer serializer)
            => ReadFont(reader);

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, Font value, JsonSerializer serializer)
            => _ = TryWrite(writer, value);


    }
}
#endif