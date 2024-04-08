#if JSON
namespace Newtonsoft.Json
{
    using System;

    /// <summary>
    /// Support custom JSON serialization and deserialization for user defined class or structure.
    /// </summary>
    public interface IJsonSerializable
    {
        /// <summary>
        /// Parse the JSON which being readed by the given <see cref="JsonReader"/> and restore the JSON contents into the current instance.
        /// </summary>
        /// <param name="jreader">The <see cref="JsonReader"/> that being reading specific JSON token where the contents is should be parsed for this object.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> that can be used to help deserialization of specific values during parsing the contents.</param>
        /// <returns>The <see cref="Int32"/> number that represents total number of fields or properties that successfuly parsed from the readed JSON data.<br/>
        /// Some of class may return the bitwise flags of the members that successfuly restored. However, zero is mean nothing.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="jreader"/> or <paramref name="serializer"/> is <see langword="null"/> reference.</exception>
        /// <exception cref="JsonException">Thrown when deserializing specific contents is failed or has invalid values.</exception>
        Int32 ReadJson(JsonReader jreader, JsonSerializer serializer);

        /// <summary>
        /// Writes the the current object with all needed contents to serialize into the specified <see cref="JsonWriter"/> output.
        /// </summary>
        /// <param name="jwriter">The output <see cref="JsonWriter"/> class that will be used to writes the current object with the contents.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> that can be used to help serialization of specific contents in this object.</param>
        /// <returns>The <see cref="Int32"/> value that represents total number of elements, properties, or fields that successfuly written.<br/>
        /// Some of class may return the bitwise flags of the members that written. However, zero is mean nothing.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="jwriter"/> or <paramref name="serializer"/> is <see langword="null"/> reference.</exception>
        /// <exception cref="JsonException">Thrown when serializing specific contents is failed or not supported.</exception>
        Int32 WriteJson(JsonWriter jwriter, JsonSerializer serializer);
    }
}
#endif