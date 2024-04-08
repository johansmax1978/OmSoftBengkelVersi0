#if JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents a default object to JSON or JSON to object converter for <see cref="CookieContainer"/> class. This class is not inheritable.
    /// </summary>
    public sealed partial class CookieContainerConverter : JsonConverter<CookieContainer>
    {
        /// <summary>
        /// Represents the default singleton instance of the <see cref="CookieContainerConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly CookieContainerConverter Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieContainerConverter"/> class.
        /// </summary>
        public CookieContainerConverter() { }

        /// <summary>
        /// Determines whether or not the current <see cref="CookieContainerConverter"/> is supporting to read the <see cref="CookieContainer"/> object from JSON.
        /// </summary>
        /// <value>By default the <see cref="CookieContainerConverter"/> class is always returning <see langword="true"/> for <see cref="CanRead"/> property.</value>
        public override Boolean CanRead => true;

        /// <summary>
        /// Determines whether or not the current <see cref="CookieContainerConverter"/> is supporting to write the <see cref="CookieContainer"/> object to JSON.
        /// </summary>
        /// <value>By default the <see cref="CookieContainerConverter"/> class is always returning <see langword="true"/> for <see cref="CanWrite"/> property.</value>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, CookieContainer value, JsonSerializer serializer) => _ = CookieConverter.TryWriteContainer(writer, value);

        /// <inheritdoc/>
        public override CookieContainer ReadJson(JsonReader reader, Type objectType, CookieContainer existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            _ = CookieConverter.TryReadContainer(reader, out var container);
            return container;
        }
    }
}
#endif