#if JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents a default object to JSON or JSON to object converter for <see cref="CookieCollection"/> class. This class is not inheritable.
    /// </summary>
    public sealed partial class CookieCollectionConverter : JsonConverter<CookieCollection>
    {
        /// <summary>
        /// Represents the default singleton instance of the <see cref="CookieCollectionConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly CookieCollectionConverter Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieCollectionConverter"/> class.
        /// </summary>
        public CookieCollectionConverter() { }

        /// <summary>
        /// Determines whether or not the current <see cref="CookieCollectionConverter"/> is supporting to read the <see cref="CookieCollection"/> object from JSON.
        /// </summary>
        /// <value>By default the <see cref="CookieCollectionConverter"/> class is always returning <see langword="true"/> for <see cref="CanRead"/> property.</value>
        public override Boolean CanRead => true;

        /// <summary>
        /// Determines whether or not the current <see cref="CookieCollectionConverter"/> is supporting to write the <see cref="CookieCollection"/> object to JSON.
        /// </summary>
        /// <value>By default the <see cref="CookieCollectionConverter"/> class is always returning <see langword="true"/> for <see cref="CanWrite"/> property.</value>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, CookieCollection value, JsonSerializer serializer) => _ = CookieConverter.TryWriteCookies(writer, value);

        /// <inheritdoc/>
        public override CookieCollection ReadJson(JsonReader reader, Type objectType, CookieCollection existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            _ = CookieConverter.TryReadCookies(reader, out var cookies);
            return cookies;
        }
    }
}
#endif