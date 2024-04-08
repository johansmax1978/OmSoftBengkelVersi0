#if JSON
namespace Newtonsoft.Json.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Reflection;

    /// <summary>
    /// Represents a default object to JSON or JSON to object converter for <see cref="Cookie"/> data type. This class is not inheritable.
    /// </summary>
    public sealed partial class CookieConverter : JsonConverter<Cookie>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly DateTime EpochBegin;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly FieldInfo TimestampField;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly FieldInfo CookieTableField;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly ReadOnlyCollection<Cookie> EmptyCookies;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const Int32 Int24MaxValue = 16777215;

        static CookieConverter()
        {
            EpochBegin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var member = typeof(Cookie).GetField("m_Timestamp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
            if (member is not null)
            {
                TimestampField = member;
            }
            else
            {
                foreach (var field in typeof(Cookie).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType == typeof(DateTime) && field.Name.IndexOf("timestamp", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        TimestampField = field;
                        break;
                    }
                }
            }
            var typeContainer = typeof(CookieContainer);
            CookieTableField = typeContainer.GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (CookieTableField is null)
            {
                foreach (var field in typeContainer.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    if (field.FieldType == typeof(Hashtable) || typeof(Hashtable).IsAssignableFrom(field.FieldType))
                    {
                        CookieTableField = field;
                        break;
                    }
                }
            }
            EmptyCookies = new ReadOnlyCollection<Cookie>(Array.Empty<Cookie>());
            Default = new();
        }

        private static Boolean GetCookies(CookieContainer container, Object key, out CookieCollection cookies, out Int32 index, out Int32 count)
        {
            index = 0;
            if (key is String domain && domain.Length != 0)
            {
                if (domain[0] == '.')
                {
                    domain = domain.Substring(1);
                    if (domain.Length == 0) goto fallback;
                }
                if ((Uri.TryCreate($"https://{domain}/", UriKind.RelativeOrAbsolute, out var uri) || Uri.TryCreate($"http://{domain}/", UriKind.RelativeOrAbsolute, out uri)) &&
                    (cookies = container.GetCookies(uri)) is not null
                    && (count = cookies.Count) != 0)
                {
                    return true;
                }
            }
        fallback:
            cookies = null;
            count = 0;
            return false;
        }

        private static IEnumerable<Cookie> ExtractCookies(CookieContainer container, Hashtable table)
        {
            Int32 index, count;
            CookieCollection cookies;
            foreach (var key in table.Keys)
            {
                if (GetCookies(container, key, out cookies, out index, out count))
                {
                    while (count-- > 0)
                    {
                        yield return cookies[index++];
                    }
                }
            }
        }

        private static Int64 GetEpoch(DateTime datetime) => datetime.Kind != DateTimeKind.Utc
                ? (Int64)Math.Round((datetime - EpochBegin.ToLocalTime()).TotalSeconds, 0, MidpointRounding.AwayFromZero)
                : (Int64)Math.Round((datetime - EpochBegin).TotalSeconds, 0, MidpointRounding.AwayFromZero);

        private static DateTime GetEpoch(Double seconds) => seconds <= 0 ? EpochBegin.ToLocalTime() : EpochBegin.ToLocalTime().AddSeconds(seconds);

        /// <summary>
        /// Utilities function, enumerate all of <see cref="Cookie"/> objects that contained in the given <see cref="CookieContainer"/> instance.
        /// </summary>
        /// <param name="container">The <see cref="CookieContainer"/> instance that supposed to be containing one or more <see cref="Cookie"/> objects.</param>
        /// <returns>A new sequence of the extracted <see cref="Cookie"/> objects, not cached in memory, forward only enumerator.</returns>
        public static IEnumerable<Cookie> ExtractCookies(CookieContainer container) => container is null || CookieTableField is not FieldInfo field
                ? (global::System.Collections.Generic.IEnumerable<global::System.Net.Cookie>)Array.Empty<Cookie>()
                : field.GetValueDirect(__makeref(container)) is Hashtable hashtable && hashtable.Count != 0
                ? ExtractCookies(container, hashtable)
                : Array.Empty<Cookie>();

        /// <summary>
        /// Collect all of <see cref="Cookie"/> objects that contained in the given <see cref="CookieContainer"/> instance and wraps in a new list.
        /// </summary>
        /// <param name="container">The <see cref="CookieContainer"/> instance that supposed to be containing one or more <see cref="Cookie"/> objects.</param>
        /// <returns>A new <see cref="ReadOnlyCollection{T}"/> of <see cref="Cookie"/> that containing collected cookies from the given <see cref="CookieContainer"/>.</returns>
        public static ReadOnlyCollection<Cookie> CollectCookies(CookieContainer container)
        {
            if (container is null || CookieTableField is not FieldInfo field || field.GetValueDirect(__makeref(container)) is not Hashtable hashtable || hashtable.Count == 0) return EmptyCookies;
            Int32 index, count;
            CookieCollection cookies;
            var iterator = hashtable.Keys.GetEnumerator();
            List<Cookie> buffer = null;
            while (iterator.MoveNext())
            {
                if (GetCookies(container, iterator.Current, out cookies, out index, out count))
                {
                    if (buffer is null) buffer = new List<Cookie>(0xff);
                    while (count-- > 0) buffer.Add(cookies[index++]);
                }
            }
            return buffer is null ? EmptyCookies : new ReadOnlyCollection<Cookie>(buffer);
        }

        /// <summary>
        /// Try to write the given <see cref="Cookie"/> object into the JSON object using the given <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> instance that should be used to write the given <see cref="Cookie"/> object.</param>
        /// <param name="cookie">The <see cref="Cookie"/> that to write into the given <see cref="JsonWriter"/> as JSON object, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the given <see cref="Cookie"/> are successfuly written to the given <see cref="JsonWriter"/> output; otherwise, <see langword="false"/>.</returns>
        public static Boolean TryWrite(JsonWriter writer, Cookie cookie)
        {
            if (writer is null)
            {
                return false;
            }
            if (cookie is null)
            {
                writer.WriteNull();
                return true;
            }
            else
            {
                cookie = new Cookie(name: "", value: "", path: "", domain: "");
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("name");
                    writer.WriteValue(cookie.Name);
                    writer.WritePropertyName("value");
                    writer.WriteValue(cookie.Value);
                    writer.WritePropertyName("path");
                    writer.WriteValue(cookie.Path);
                    writer.WritePropertyName("domain");
                    writer.WriteValue(cookie.Domain);
                    writer.WritePropertyName("port");
                    writer.WriteValue(cookie.Port);
                    writer.WritePropertyName("httponly");
                    writer.WriteValue(cookie.HttpOnly);
                    writer.WritePropertyName("secure");
                    writer.WriteValue(cookie.Secure);
                    if (cookie.Expires != DateTime.MinValue)
                    {
                        writer.WritePropertyName("expires");
                        writer.WriteValue(GetEpoch(cookie.Expires));
                    }
                    else
                    {
                        writer.WritePropertyName("expires");
                        writer.WriteValue(0);
                    }
                    writer.WritePropertyName("expired");
                    writer.WriteValue(cookie.Expired);
                    if (cookie.TimeStamp > DateTime.MinValue)
                    {
                        writer.WritePropertyName("timestamp");
                        writer.WriteValue(GetEpoch(cookie.TimeStamp));
                    }
                    else
                    {
                        writer.WritePropertyName("timestamp");
                        writer.WriteValue(0);
                    }
                    writer.WritePropertyName("version");
                    writer.WriteValue(cookie.Version);
                    writer.WritePropertyName("discard");
                    writer.WriteValue(cookie.Discard);
                    writer.WritePropertyName("comment");
                    writer.WriteValue(cookie.Comment);
                    writer.WritePropertyName("commenturi");
                    writer.WriteValue(cookie.CommentUri);
                }
                writer.WriteEndObject();
                return true;
            }
        }

        /// <summary>
        /// Try to parse the current readed JSON object token into the <see cref="Cookie"/> class.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that positioned at the <see cref="JsonToken.StartObject"/>.</param>
        /// <param name="cookie">Retrieve the parsed <see cref="Cookie"/> instance, or <see langword="null"/> when parsing is failed.</param>
        /// <returns><see langword="true"/> if the <see cref="Cookie"/> has successfuly parsed from <see cref="JsonReader"/>, or <see langword="false"/> otherwise.</returns>
        public static Boolean TryRead(JsonReader reader, out Cookie cookie)
        {
            if (reader is null)
            {
                cookie = null;
                return false;
            }
            if (reader.TokenType == JsonToken.None && !reader.Read())
            {
                cookie = null;
                return false;
            }
            else if (reader.TokenType is JsonToken.Null or JsonToken.Undefined or JsonToken.None)
            {
                cookie = null;
                return true;
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                const Int32 HAS_NAME = 0x1, HAS_VALUE = 0x2, HAS_DOMAIN = 0x4, HAS_PATH = 0x8, MIN_ACCEPT = 0x1 | 0x2;
                var flags = 0x0;
                cookie = new Cookie();
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var property = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                        if (reader.Read() && property is not null && property.Length != 0)
                        {
                            var value = reader.Value;
                            switch (property.ToLower())
                            {
                                case "name":
                                    cookie.Name = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    if (!String.IsNullOrEmpty(cookie.Name)) flags |= HAS_NAME;
                                    break;
                                case "value":
                                    cookie.Value = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    if (!String.IsNullOrEmpty(cookie.Value)) flags |= HAS_VALUE;
                                    break;
                                case "domain":
                                    cookie.Domain = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    if (!String.IsNullOrEmpty(cookie.Domain)) flags |= HAS_DOMAIN;
                                    break;
                                case "path":
                                    cookie.Path = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    if (String.IsNullOrEmpty(cookie.Path)) cookie.Path = "/";
                                    flags |= HAS_PATH;
                                    break;
                                case "port":
                                    cookie.Port = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    break;
                                case "httponly":
                                case "http":
                                case "http-only":
                                case "http_only":
                                    cookie.HttpOnly = Convert.ToBoolean(value, CultureInfo.CurrentCulture);
                                    break;
                                case "secure":
                                case "secured":
                                    cookie.Secure = Convert.ToBoolean(value, CultureInfo.CurrentCulture);
                                    break;
                                case "expires":
                                    if (reader.TokenType != JsonToken.Null)
                                    {
                                        if (reader.TokenType == JsonToken.String)
                                        {
                                            var expstr = Convert.ToString(value, CultureInfo.CurrentCulture);
                                            if (DateTime.TryParse(expstr, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.RoundtripKind, out var date))
                                            {
                                                cookie.Expires = date;
                                            }
                                            else if (DateTime.TryParseExact(expstr, "Y-m-d H:m:s", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.RoundtripKind, out date))
                                            {
                                                cookie.Expires = date;
                                            }
                                            else if (DateTime.TryParseExact(expstr, "Y-m-d H:m:s.fffff", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.RoundtripKind, out date))
                                            {
                                                cookie.Expires = date;
                                            }
                                        }
                                        else if (reader.TokenType == JsonToken.Float)
                                        {
                                            cookie.Expires = GetEpoch(Convert.ToDouble(value, CultureInfo.CurrentCulture));
                                        }
                                        else if (reader.TokenType == JsonToken.Integer)
                                        {
                                            cookie.Expires = GetEpoch(Convert.ToInt64(value, CultureInfo.CurrentCulture));
                                        }
                                    }
                                    break;
                                case "expired":
                                    cookie.Expired = Convert.ToBoolean(value, CultureInfo.CurrentCulture);
                                    break;
                                case "discard":
                                    cookie.Discard = Convert.ToBoolean(value, CultureInfo.CurrentCulture);
                                    break;
                                case "comment":
                                    cookie.Comment = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    break;
                                case "commenturi":
                                case "commenturl":
                                case "comment_uri":
                                case "comment_url":
                                case "comment-uri":
                                case "comment-url":
                                    var uristr = Convert.ToString(value, CultureInfo.CurrentCulture);
                                    if (Uri.TryCreate(uristr, UriKind.RelativeOrAbsolute, out var uri))
                                        cookie.CommentUri = uri;
                                    break;
                            }
                        }
                    }
                }
                if ((flags & MIN_ACCEPT) != 0x0) return true;
            }
            cookie = null;
            return false;
        }

        /// <summary>
        /// Try to read the current readed JSON array which represents the <see cref="Cookie"/> arrays and create a new <see cref="CookieContainer"/> to store the parsed cookies.
        /// </summary>
        /// <param name="reader">Required, the <see cref="JsonReader"/> that currently positioned at <see cref="JsonToken.StartArray"/> to parse all of JSON array elements as <see cref="Cookie"/> instances.</param>
        /// <param name="container">Get a new instance of the <see cref="CookieContainer"/> that containing zero or more <see cref="Cookie"/> objects which parsed from JSON, or <see langword="null"/> on failed.</param>
        /// <returns>The <see cref="Int32"/> value as the total number of <see cref="Cookie"/> objects that parsed from the readed JSON array using the given <see cref="JsonReader"/>, or zero if nothing.</returns>
        public static Int32 TryReadContainer(JsonReader reader, out CookieContainer container)
        {
            if (reader is null || (reader.TokenType == JsonToken.None && !reader.Read()))
            {
                container = null;
                return 0;
            }
            else if (reader.TokenType == JsonToken.StartArray && CookieTableField is FieldInfo field)
            {
                var found = 0;
                container = new CookieContainer(Int24MaxValue);
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.StartObject && TryRead(reader, out var cookie))
                    {
                        container.Add(cookie);
                        found++;
                    }
                }
                return found;
            }
            else
            {
                container = new CookieContainer();
                return 0;
            }
        }

        /// <summary>
        /// Try to write entire <see cref="Cookie"/> objects that contained in the given <see cref="CookieContainer"/> as JSON array using the given <see cref="JsonWriter"/> instance.
        /// </summary>
        /// <param name="writer">Required, the <see cref="JsonWriter"/> that should be used to write entire <see cref="Cookie"/> entries in the given <see cref="CookieContainer"/> as JSON array.</param>
        /// <param name="container">The <see cref="CookieContainer"/> instance to extract all of the <see cref="Cookie"/> objects and write as JSON array using the given <see cref="JsonWriter"/>.</param>
        /// <returns>The <see cref="Int32"/> value as the count of the <see cref="Cookie"/> objects in the given <see cref="CookieContainer"/> that successfuly writen as JSON array elements.</returns>
        public static Int32 TryWriteContainer(JsonWriter writer, CookieContainer container)
        {
            if (writer is null)
            {
                return 0;
            }
            else if (container is null)
            {
                writer.WriteNull();
                return 0;
            }
            else if (CookieTableField is FieldInfo field)
            {
                var hashtable = field.GetValueDirect(__makeref(container)) as Hashtable;
                writer.WriteStartArray();
                if (hashtable is not null && hashtable.Count != 0)
                {
                    using var iterator = ExtractCookies(container, hashtable).GetEnumerator();
                    while (iterator.MoveNext()) _ = TryWrite(writer, iterator.Current);
                }
                writer.WriteEndArray();
                return hashtable is null ? 0 : hashtable.Count;
            }
            else
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
                return 0;
            }
        }

        /// <summary>
        /// Try to read the current readed JSON array which represents the <see cref="Cookie"/> arrays and create a new <see cref="CookieCollection"/> to store the parsed cookies.
        /// </summary>
        /// <param name="reader">Required, the <see cref="JsonReader"/> that currently positioned at <see cref="JsonToken.StartArray"/> to parse all of JSON array elements as <see cref="Cookie"/> instances.</param>
        /// <param name="cookies">Get a new instance of the <see cref="CookieCollection"/> that containing zero or more <see cref="Cookie"/> objects which parsed from JSON, or <see langword="null"/> on failed.</param>
        /// <returns>The <see cref="Int32"/> value as the total number of <see cref="Cookie"/> objects that parsed from the readed JSON array using the given <see cref="JsonReader"/>, or zero if nothing.</returns>
        public static Int32 TryReadCookies(JsonReader reader, out CookieCollection cookies)
        {
            if (reader is null || (reader.TokenType == JsonToken.None && !reader.Read()))
            {
                cookies = null;
                return 0;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var storage = new CookieCollection();
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.StartObject && TryRead(reader, out var cookie))
                    {
                        storage.Add(cookie);
                    }
                }
                cookies = storage;
                return storage.Count;
            }
            else
            {
                cookies = null;
                return 0;
            }
        }

        /// <summary>
        /// Try to write entire <see cref="Cookie"/> objects that contained in the given <see cref="CookieCollection"/> as JSON array using the given <see cref="JsonWriter"/> instance.
        /// </summary>
        /// <param name="writer">Required, the <see cref="JsonWriter"/> that should be used to write entire <see cref="Cookie"/> entries in the given <see cref="CookieCollection"/> as JSON array.</param>
        /// <param name="cookies">The <see cref="CookieCollection"/> instance to extract all of the <see cref="Cookie"/> objects and write as JSON array using the given <see cref="JsonWriter"/>.</param>
        /// <returns>The <see cref="Int32"/> value as the count of the <see cref="Cookie"/> objects in the given <see cref="CookieCollection"/> that successfuly writen as JSON array elements.</returns>
        public static Int32 TryWriteCookies(JsonWriter writer, CookieCollection cookies)
        {
            if (cookies is not null && writer is not null)
            {
                writer.WriteStartArray();
                for (var i = 0; i < cookies.Count; _ = TryWrite(writer, cookies[i++])) ;
                writer.WriteEndArray();
                return cookies.Count;
            }
            return 0;
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="CookieConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly CookieConverter Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieConverter"/> class.
        /// </summary>
        public CookieConverter() { }

        /// <summary>
        /// Determines whether or not the current <see cref="CookieConverter"/> is supporting to read the <see cref="Cookie"/> object from JSON.
        /// </summary>
        /// <value>By default the <see cref="CookieConverter"/> class is always returning <see langword="true"/> for <see cref="CanRead"/> property.</value>
        public override Boolean CanRead => true;

        /// <summary>
        /// Determines whether or not the current <see cref="CookieConverter"/> is supporting to write the <see cref="Cookie"/> object to JSON.
        /// </summary>
        /// <value>By default the <see cref="CookieConverter"/> class is always returning <see langword="true"/> for <see cref="CanWrite"/> property.</value>
        public override Boolean CanWrite => true;

        /// <inheritdoc/>
        public override Cookie ReadJson(JsonReader reader, Type objectType, Cookie existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            if (TryRead(reader, out var cookie))
            {
                var useExisting = hasExistingValue && serializer is not null && serializer.ObjectCreationHandling == ObjectCreationHandling.Reuse;
                if (useExisting)
                {
                    existingValue.Name = cookie.Name;
                    existingValue.Value = cookie.Value;
                    existingValue.Path = cookie.Path;
                    existingValue.Port = cookie.Port;
                    existingValue.Secure = cookie.Secure;
                    existingValue.HttpOnly = cookie.HttpOnly;
                    existingValue.Comment = cookie.Comment;
                    existingValue.CommentUri = cookie.CommentUri;
                    existingValue.Discard = cookie.Discard;
                    existingValue.Domain = cookie.Domain;
                    existingValue.Expires = cookie.Expires;
                    existingValue.Expired = cookie.Expired;
                    existingValue.Version = cookie.Version;
                    if (TimestampField is FieldInfo field) field.SetValueDirect(__makeref(existingValue), field.GetValueDirect(__makeref(cookie)));
                    return existingValue;
                }
                return cookie;
            }
            return null;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, Cookie value, JsonSerializer serializer) => _ = TryWrite(writer, value);
    }
}
#endif