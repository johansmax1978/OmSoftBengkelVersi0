namespace Newtonsoft.Json.Converters
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Numerics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a <see cref="JsonConverter"/> for any objects that implements the <see cref="IJsonSerializable"/> interface.
    /// </summary>
    public sealed partial class JsonSerializableConverter : JsonConverter
    {
        [Flags]
        private enum CtorFlags { None = 0x0, Parameterless = 0x1, WithJsonReader = 0x2, WithJsonReaderJsonSerializer = 0x4, WithSerializationInfo = 0x8, WithSerializationInfoStreamingContext = 0x10 }

        private sealed partial class RestoreMapping
        {
            private static CtorFlags FindType(ParameterInfo[] args) => args.Length == 0
                    ? CtorFlags.Parameterless
                    : args.Length == 1
                        ? args[0].ParameterType == typeof(JsonReader) ? CtorFlags.WithJsonReader : args[0].ParameterType == typeof(SerializationInfo) ? CtorFlags.WithSerializationInfo : CtorFlags.None
                        : args.Length == 2
                            ? args[0].ParameterType == typeof(JsonReader)
                                ? args[1].ParameterType == typeof(JsonSerializer) ? CtorFlags.WithJsonReaderJsonSerializer : CtorFlags.None
                                : args[0].ParameterType == typeof(SerializationInfo)
                                    ? args[1].ParameterType == typeof(StreamingContext) ? CtorFlags.WithSerializationInfoStreamingContext : CtorFlags.None
                                    : CtorFlags.None
                        : CtorFlags.None;

            private RestoreMapping(Type type)
            {
                CtorFlags flags = CtorFlags.None, next;
                foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    switch (next = FindType(ctor.GetParameters()))
                    {
                        case CtorFlags.Parameterless:
                            this.Parameterless = ctor;
                            flags |= next;
                            break;
                        case CtorFlags.WithJsonReader:
                            this.WithJsonReader = ctor;
                            flags |= next;
                            break;
                        case CtorFlags.WithJsonReaderJsonSerializer:
                            this.WithJsonReaderJsonSerializer = ctor;
                            flags |= next;
                            break;
                        case CtorFlags.WithSerializationInfo:
                            this.WithSerializationInfo = ctor;
                            flags |= next;
                            break;
                        case CtorFlags.WithSerializationInfoStreamingContext:
                            this.WithSerializationInfoStreamingContext = ctor;
                            flags |= next;
                            break;
                    }
                }
                this.Type = type;
                this.Flags = flags;
            }

            public readonly Type Type;
            public readonly CtorFlags Flags;
            public readonly ConstructorInfo Parameterless;
            public readonly ConstructorInfo WithJsonReader;
            public readonly ConstructorInfo WithJsonReaderJsonSerializer;
            public readonly ConstructorInfo WithSerializationInfo;
            public readonly ConstructorInfo WithSerializationInfoStreamingContext;

            private static readonly Dictionary<Int32, RestoreMapping> Collection;

            static RestoreMapping() => Collection = new(0x10, EqualityComparer<Int32>.Default);

            public static RestoreMapping FromType(Type type)
            {
                var hash = RuntimeHelpers.GetHashCode(type);
                if (!Collection.TryGetValue(hash, out var ctor)) Collection[hash] = ctor = new RestoreMapping(type);
                return ctor;
            }

            public static Object Normalize(JsonReader reader, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Boolean:
                        return Convert.ToBoolean(reader.Value, CultureInfo.CurrentCulture);
                    case JsonToken.Integer:
                        if (reader.Value is BigInteger bigint)
                        {
                            return bigint;
                        }
                        else
                        {
                            switch (Type.GetTypeCode(reader.Value.GetType()))
                            {
                                case TypeCode.Int16:
                                    return Convert.ToInt16(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.Int32:
                                    return Convert.ToInt32(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.Int64:
                                    return Convert.ToInt64(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.SByte:
                                    return Convert.ToSByte(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.UInt16:
                                    return Convert.ToUInt16(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.UInt32:
                                    return Convert.ToUInt32(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.UInt64:
                                    return Convert.ToUInt64(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.Byte:
                                    return Convert.ToByte(reader.Value, CultureInfo.CurrentCulture);
                                case TypeCode.String:
                                    var format = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                                    if (BigInteger.TryParse(format, NumberStyles.Integer, CultureInfo.CurrentCulture, out bigint) || BigInteger.TryParse(format, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out bigint))
                                    {
                                        if (bigint <= Int64.MaxValue && bigint >= Int64.MinValue)
                                            return (Int64)bigint;
                                        else if (bigint <= Int32.MaxValue && bigint >= Int32.MinValue)
                                            return (Int32)bigint;
                                        else if (bigint >= BigInteger.Zero && bigint <= UInt64.MaxValue)
                                            return (UInt64)bigint;
                                        else
                                            return bigint;
                                    }
                                    return format;
                                default:
                                    goto fallback;
                            }
                        }
                    case JsonToken.Float:
                        try
                        {
                            return Convert.ToDouble(reader.Value, CultureInfo.CurrentCulture);
                        }
                        catch
                        {
                            return Convert.ToDecimal(reader.Value, CultureInfo.CurrentCulture);
                        }
                    case JsonToken.Bytes:
                        if (reader.Value is Byte[] bytes)
                            return bytes;
                        else if (reader.Value is String base64)
                            return Convert.FromBase64String(base64);
                        else if (reader.Value is JToken token)
                            return token.ToObject<Byte[]>(serializer);
                        else
                            goto fallback;
                    case JsonToken.Date:
                        if (reader.DateParseHandling == DateParseHandling.DateTimeOffset)
                        {
                            if (reader.Value is JToken token)
                                return token.ToObject<DateTimeOffset>(serializer);
                            else if (reader.Value is DateTimeOffset datetimeoffset)
                                return datetimeoffset;
                            else if (reader.Value is DateTime datetime)
                                return new DateTimeOffset(datetime, DateTimeOffset.Now.Offset);
                            else
                                return serializer.Deserialize<DateTimeOffset>(reader);
                        }
                        else
                        {
                            if (reader.Value is JToken token)
                                return token.ToObject<DateTime>(serializer);
                            else if (reader.Value is DateTime datetime)
                                return datetime;
                            else if (reader.Value is DateTimeOffset datetimeoffset)
                                return datetimeoffset.LocalDateTime;
                            else
                                return serializer.Deserialize<DateTime>(reader);
                        }

                    case JsonToken.None:
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        return null;
                    case JsonToken.StartObject:
                        return JObject.Load(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace, LineInfoHandling = LineInfoHandling.Ignore });
                    case JsonToken.StartArray:
                        return JArray.Load(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace, LineInfoHandling = LineInfoHandling.Ignore });
                    case JsonToken.StartConstructor:
                        return JConstructor.Load(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace, LineInfoHandling = LineInfoHandling.Ignore });
                    case JsonToken.PropertyName:
                        return Convert.ToString(reader.Value);
                    case JsonToken.Comment:
                    case JsonToken.Raw:
                        return reader.Value?.ToString();
                    case JsonToken.String:
                        return Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                    case JsonToken.EndConstructor:
                        return null;
                }
            fallback:
                return reader.Value;
            }
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="JsonSerializableConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly JsonSerializableConverter Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializableConverter"/> class.
        /// </summary>
        public JsonSerializableConverter() { }

        /// <inheritdoc/>
        public override Boolean CanRead => true;

        /// <inheritdoc/>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        public override Boolean CanConvert(Type objectType) => objectType is null || typeof(IJsonSerializable).IsAssignableFrom(objectType);

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
        {
            if (value is null)
                writer.WriteNull();
            else _ = value is IJsonSerializable ijs
                ? ijs.WriteJson(writer, serializer)
                : throw new JsonException($"The {nameof(JsonSerializableConverter)} can only serialize any objects that implements {nameof(IJsonSerializable)} interface.");
        }

        /// <inheritdoc/>
        public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        {
            if (objectType is null || typeof(JToken).IsAssignableFrom(objectType))
            {
                return JToken.ReadFrom(reader, new JsonLoadSettings
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                    LineInfoHandling = LineInfoHandling.Load
                });
            }
            else if (this.CanConvert(objectType))
            {
                if (existingValue is not null && serializer is not null && serializer.ObjectCreationHandling != ObjectCreationHandling.Replace)
                {
                    if (existingValue is IJsonSerializable serializable)
                    {
                        _ = serializable.ReadJson(reader, serializer);
                        return existingValue;
                    }
                }
                if (objectType.IsAbstract)
                {
                    throw new JsonException($"Cannot create a new instance of the abstract (MustInherits in Visual Basic) class: \"{objectType}\".");
                }
                var ctor = RestoreMapping.FromType(objectType);
                if (ctor.Flags == CtorFlags.None)
                {
                    throw new JsonException($"There is no suite constructor to construct the object of type \"{objectType}\" as for JSON deserialization purpose. Try to define at least one of the private or public parameterless constructor.");
                }
                else if ((ctor.Flags & CtorFlags.WithJsonReaderJsonSerializer) != CtorFlags.None)
                {
                    return ctor.WithJsonReaderJsonSerializer.Invoke(new Object[] { reader, serializer });
                }
                else if ((ctor.Flags & CtorFlags.WithJsonReader) != CtorFlags.None)
                {
                    return ctor.WithJsonReader.Invoke(new Object[] { reader });
                }
                else if ((ctor.Flags & CtorFlags.Parameterless) != CtorFlags.None)
                {
                    existingValue = ctor.Parameterless.Invoke(Array.Empty<Object>());
                    _ = ((IJsonSerializable)existingValue).ReadJson(reader, serializer);
                    return existingValue;
                }
                else
                {
                    StreamingContext context;
                    SerializationInfo info;
                    try
                    {
                        context = serializer is null ? new StreamingContext(StreamingContextStates.File) : serializer.Context;
                        info = new SerializationInfo(objectType, new FormatterConverter(), false);
                        if ((ctor.Flags & CtorFlags.WithSerializationInfoStreamingContext) != CtorFlags.None)
                        {
                            existingValue = ctor.WithSerializationInfoStreamingContext.Invoke(new Object[] { info, context });
                        }
                        else
                        {
                            existingValue = ctor.WithSerializationInfo.Invoke(new Object[] { info });
                        }
                        _ = ((IJsonSerializable)existingValue).ReadJson(reader, serializer);
                        return existingValue;
                    }
                    catch
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            context = serializer is null ? new StreamingContext(StreamingContextStates.File) : serializer.Context;
                            info = new SerializationInfo(objectType, new FormatterConverter(), false);
                            if (serializer is null) serializer = JsonSerializer.CreateDefault();
                            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                            {
                                if (reader.TokenType != JsonToken.PropertyName) continue;
                                var property = Convert.ToString(reader.Value);
                                if (!reader.Read()) break;
                                var value = RestoreMapping.Normalize(reader, serializer);
                                info.AddValue(property, value, value is null ? typeof(Object) : value.GetType());
                            }
                        }
                        else if (reader.TokenType == JsonToken.StartArray)
                        {
                            context = serializer is null ? new StreamingContext(StreamingContextStates.File) : serializer.Context;
                            info = new SerializationInfo(objectType, new FormatterConverter(), false);
                            var array = JArray.Load(reader, new JsonLoadSettings
                            {
                                CommentHandling = CommentHandling.Ignore,
                                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                                LineInfoHandling = LineInfoHandling.Load
                            });
                            info.AddValue("items", array, typeof(JArray));
                        }
                        else
                        {
                            throw new JsonException($"Failed to restore the object of type \"{objectType}\" from JSON token \"{reader.TokenType}\".");
                        }
                        if ((ctor.Flags & CtorFlags.WithSerializationInfoStreamingContext) != CtorFlags.None)
                        {
                            return ctor.WithSerializationInfoStreamingContext.Invoke(new Object[] { info, context });
                        }
                        else
                        {
                            return ctor.WithSerializationInfo.Invoke(new Object[] { info });
                        }
                    }
                }
            }
            else
            {
                throw new JsonException($"The \"{nameof(JsonSerializableConverter)}\" can only deserialize any objects that implements \"{nameof(IJsonSerializable)}\" interface.");
            }
        }

    }
}