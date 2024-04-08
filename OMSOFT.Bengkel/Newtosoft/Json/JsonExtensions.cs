namespace Newtonsoft.Json
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Numerics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for common JSON reading and writing operations, this class is not inheritable.
    /// </summary>
    public static partial class JsonExtensions
    {
        /// <summary>
        /// Asynchronously stream the current <see cref="HttpContent"/> by creating the <see cref="JsonTextReader"/> to read or parse the donwloaded JSON contents.
        /// </summary>
        /// <param name="content">Required, specify the <see cref="HttpContent"/> that facilitating read access to the downloaded content from the underlying HTTP request.</param>
        /// <returns>The <see cref="Task{TResult}"/> that represents the asynchronous operation which resulting <see cref="JsonTextReader"/> that reading the downloaded content.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="content"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static Task<JsonTextReader> ReadJsonAsync(this HttpContent content) => ReadJsonAsync(content, null, 0);

        /// <summary>
        /// Asynchronously stream the current <see cref="HttpContent"/> by creating the <see cref="JsonTextReader"/> to read or parse the donwloaded JSON contents.
        /// </summary>
        /// <param name="content">Required, specify the <see cref="HttpContent"/> that facilitating read access to the downloaded content from the underlying HTTP request.</param>
        /// <param name="encoding">Optional, specify the <see cref="Encoding"/> to be used on characters set decoding, -or set with <see langword="null"/> to use UTF-8 encoding.</param>
        /// <returns>The <see cref="Task{TResult}"/> that represents the asynchronous operation which resulting <see cref="JsonTextReader"/> that reading the downloaded content.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="content"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static Task<JsonTextReader> ReadJsonAsync(this HttpContent content, Encoding encoding) => ReadJsonAsync(content, encoding, 0);

        /// <summary>
        /// Asynchronously stream the current <see cref="HttpContent"/> by creating the <see cref="JsonTextReader"/> to read or parse the donwloaded JSON contents.
        /// </summary>
        /// <param name="content">Required, specify the <see cref="HttpContent"/> that facilitating read access to the downloaded content from the underlying HTTP request.</param>
        /// <param name="encoding">Optional, specify the <see cref="Encoding"/> to be used on characters set decoding, -or set with <see langword="null"/> to use UTF-8 encoding.</param>
        /// <param name="bufferSize">Optional, specify the number of buffer (either bytes or characters) to read per read loops. The default is 8192 bytes.</param>
        /// <returns>The <see cref="Task{TResult}"/> that represents the asynchronous operation which resulting <see cref="JsonTextReader"/> that reading the downloaded content.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="content"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static async Task<JsonTextReader> ReadJsonAsync(this HttpContent content, Encoding encoding, Int32 bufferSize)
        {
            if (content is null) throw new ArgumentNullException(nameof(content), "The HTTP-Content must not be null reference.");
            Stream stream;
            TextReader reader;
            if (content is StringContent @string)
            {
                String json = await @string.ReadAsStringAsync().ConfigureAwait(false);
                reader = new StringReader(json);
            }
            else if (content is ByteArrayContent bytes)
            {
                var array = await bytes.ReadAsByteArrayAsync().ConfigureAwait(false);
                stream = new MemoryStream(array);
                reader = new StreamReader(stream, encoding ?? JsonDefault.CreateEncoding(), true, bufferSize < 1 ? 8192 : bufferSize, false);
            }
            else
            {
                stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
                reader = new StreamReader(stream, encoding ?? JsonDefault.CreateEncoding(), true, bufferSize < 1 ? 8192 : bufferSize, false);
            }
            return ReadJson(reader, true);

        }

        /// <summary>
        /// Open the stream to retrieve the current HTTP response data and attach a new <see cref="JsonTextReader"/> to parse the contents from JSON format.
        /// </summary>
        /// <param name="response">Required, specify the <see cref="HttpWebResponse"/> that already sent and begining to retrieve the HTTP response data to read as JSON.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that reading JSON source by streaming the requested HTTP response data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="response"/> is <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextReader GetResponseAsJson(this HttpWebResponse response) => GetResponseAsJson(response, null, 0);

        /// <summary>
        /// Open the stream to retrieve the current HTTP response data and attach a new <see cref="JsonTextReader"/> to parse the contents from JSON format.
        /// </summary>
        /// <param name="response">Required, specify the <see cref="HttpWebResponse"/> that already sent and begining to retrieve the HTTP response data to read as JSON.</param>
        /// <param name="encoding">Optional, specify the <see cref="Encoding"/> object to be used on characters set decoding, -or set with <see langword="null"/> to use UTF-8 encoding (default).</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that reading JSON source by streaming the requested HTTP response data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="response"/> is <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextReader GetResponseAsJson(this HttpWebResponse response, Encoding encoding) => GetResponseAsJson(response, encoding, 0);

        /// <summary>
        /// Open the stream to retrieve the current HTTP response data and attach a new <see cref="JsonTextReader"/> to parse the contents from JSON format.
        /// </summary>
        /// <param name="response">Required, specify the <see cref="HttpWebResponse"/> that already sent and begining to retrieve the HTTP response data to read as JSON.</param>
        /// <param name="encoding">Optional, specify the <see cref="Encoding"/> object to be used on characters set decoding, -or set with <see langword="null"/> to use UTF-8 encoding (default).</param>
        /// <param name="bufferSize">Optional, specify the number of buffer (either bytes or characters) to read per read loops. The default buffer size is 8192 bytes.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that reading JSON source by streaming the requested HTTP response data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="response"/> is <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextReader GetResponseAsJson(this HttpWebResponse response, Encoding encoding, Int32 bufferSize)
            => response is null ? throw new ArgumentNullException(nameof(response), "Cannot get JSON response from null reference.") : ReadJson(response.GetResponseStream(), encoding, bufferSize, true);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the minified JSON data into the underlying HTTP destination using the default preferences.
        /// </summary>
        /// <param name="request">Required, the <see cref="HttpWebRequest"/> instance to begin sending HTTP request data using <see cref="JsonTextWriter"/> as for writing the JSON data.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="request"/> has retrieving HTTP response.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextWriter GetRequestAsJson(this HttpWebRequest request) => GetRequestAsJson(request, false, true, null, 0);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying HTTP destination using specified preferences.
        /// </summary>
        /// <param name="request">Required, the <see cref="HttpWebRequest"/> instance to begin sending HTTP request data using <see cref="JsonTextWriter"/> as for writing the JSON data.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="request"/> has retrieving HTTP response.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextWriter GetRequestAsJson(this HttpWebRequest request, Boolean pretified) => GetRequestAsJson(request, pretified, true, null, 0);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying HTTP destination using specified preferences.
        /// </summary>
        /// <param name="request">Required, the <see cref="HttpWebRequest"/> instance to begin sending HTTP request data using <see cref="JsonTextWriter"/> as for writing the JSON data.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="request"/> has retrieving HTTP response.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextWriter GetRequestAsJson(this HttpWebRequest request, Boolean pretified, Boolean escaping) => GetRequestAsJson(request, pretified, escaping, null, 0);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying HTTP destination using specified preferences.
        /// </summary>
        /// <param name="request">Required, the <see cref="HttpWebRequest"/> instance to begin sending HTTP request data using <see cref="JsonTextWriter"/> as for writing the JSON data.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <param name="encoding">Optional, set with <see cref="Encoding"/> object that should be used to encode all writen characters into the bytes data, the default is <see cref="UTF8Encoding"/>.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="request"/> has retrieving HTTP response.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextWriter GetRequestAsJson(this HttpWebRequest request, Boolean pretified, Boolean escaping, Encoding encoding) => GetRequestAsJson(request, pretified, escaping, encoding, 0);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying HTTP destination using specified preferences.
        /// </summary>
        /// <param name="request">Required, the <see cref="HttpWebRequest"/> instance to begin sending HTTP request data using <see cref="JsonTextWriter"/> as for writing the JSON data.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <param name="encoding">Optional, set with <see cref="Encoding"/> object that should be used to encode all writen characters into the bytes data, the default is <see cref="UTF8Encoding"/>.</param>
        /// <param name="bufferSize">Optional, the number of bytes to cache in the memory before it writen to the underlying <see cref="Stream"/> output, the default is 8KB/cycles.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="request"/> has retrieving HTTP response.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextWriter GetRequestAsJson(this HttpWebRequest request, Boolean pretified, Boolean escaping, Encoding encoding, Int32 bufferSize)
            => request is null ? throw new ArgumentNullException(nameof(request), "Cannot create HTTP-Request stream as for written by JSON writer from null reference.") : WriteJson(request.GetRequestStream(), pretified, escaping, encoding ?? JsonDefault.CreateEncoding(), bufferSize, true);

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that parse the JSON content from the current JSON <see cref="String"/> content.
        /// </summary>
        /// <param name="json">Required, specify the <see cref="String"/> that represents the valid JSON format to read and parse.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="json"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextReader ReadJson(this String json)
        {
            var reader = new StringReader(json);
            return new JsonTextReader(reader)
            {
                CloseInput = true,
                Culture = JsonDefault.DefaultCultureInfo,
                DateParseHandling = DateParseHandling.DateTime,
                FloatParseHandling = FloatParseHandling.Double,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                SupportMultipleContent = true
            };
        }

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that parse the JSON content from the current JSON <see cref="String"/> content.
        /// </summary>
        /// <param name="json">Required, specify the <see cref="String"/> that represents the valid JSON format to read and parse.</param>
        /// <param name="start">The index of the character in the <see cref="String"/> to begin read, a negative value mean tailing index.</param>
        /// <param name="length">The length of characters to read from the <see cref="String"/>, a negative value mean <see cref="String.Length"/>-n.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> that reading JSON source from the given input <see cref="String"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="json"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        public static JsonTextReader ReadJson(this String json, Int32 start, Int32 length)
        {
            if (json is null) throw new ArgumentNullException(nameof(json), "The JSON string must not null reference.");
            var avail = json.Length;
            if (start < 0 && (start = avail + start) < 0 || start >= avail)
                throw new ArgumentOutOfRangeException(nameof(start), start, "The starting character cursor is out of range.");
            if (length < 0)
            {
                length = (avail + length) - start;
                if (length < 0 || length > avail) throw new ArgumentOutOfRangeException(nameof(length), length, "The length of characters is out of range.");
            }
            if (start + length > avail) throw new ArgumentOutOfRangeException($"{nameof(start)}, {nameof(length)}", $"{start}, {length}", "The range of characters is out of range.");
            var reader = new StringReader(length == 0 ? "" : json.Substring(start, length));
            return new JsonTextReader(reader)
            {
                CloseInput = true,
                Culture = JsonDefault.DefaultCultureInfo,
                DateParseHandling = DateParseHandling.DateTime,
                FloatParseHandling = FloatParseHandling.Double,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                SupportMultipleContent = true
            };
        }

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that parse the JSON content from the text that being readed in the given <see cref="TextReader"/> object.
        /// </summary>
        /// <param name="reader">Required, specify the <see cref="TextReader"/> object that being reading the JSON text from the underlying source to parse.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> that reading JSON source from the streaming data which readed by the given <see cref="TextReader"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextReader ReadJson(this TextReader reader) => new(reader)
        {
            ArrayPool = JsonCharsPool.Instance,
            CloseInput = false,
            Culture = JsonDefault.DefaultCultureInfo,
            DateParseHandling = DateParseHandling.DateTime,
            FloatParseHandling = FloatParseHandling.Double,
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            SupportMultipleContent = true
        };

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that parse the JSON content from the text that being readed in the given <see cref="TextReader"/> object.
        /// </summary>
        /// <param name="reader">Required, specify the <see cref="TextReader"/> object that being reading the JSON text from the underlying source to parse.</param>
        /// <param name="disposable">Optional, set with <see langword="true"/> to dispose the <see cref="TextReader"/> together with <see cref="JsonTextReader"/>; otherwise, set <see langword="false"/> (default).</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> that reading JSON source from the streaming data which readed by the given <see cref="TextReader"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextReader ReadJson(this TextReader reader, Boolean disposable) => new(reader)
        {
            ArrayPool = JsonCharsPool.Instance,
            CloseInput = disposable,
            Culture = JsonDefault.DefaultCultureInfo,
            DateParseHandling = DateParseHandling.DateTime,
            FloatParseHandling = FloatParseHandling.Double,
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            SupportMultipleContent = true
        };

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that reading JSON content from the given <see cref="Stream"/> using default UTF-8 <see cref="Encoding"/>.
        /// </summary>
        /// <param name="stream">Required, specify the readable <see cref="Stream"/> that currently is being streaming the JSON content to read.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that can be used to read the JSON contents from the given <see cref="Stream"/> buffer.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not readable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextReader ReadJson(this Stream stream) => ReadJson(stream, null, 0, false);

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that reading JSON content from the given <see cref="Stream"/> using specified <see cref="Encoding"/> type.
        /// </summary>
        /// <param name="stream">Required, specify the readable <see cref="Stream"/> that currently is being streaming the JSON content to read.</param>
        /// <param name="encoding">Optional, the <see cref="Encoding"/> object that should be used to decode bytes data into the desired characters.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that can be used to read the JSON contents from the given <see cref="Stream"/> buffer.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not readable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextReader ReadJson(this Stream stream, Encoding encoding) => ReadJson(stream, encoding, 0, false);

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that reading JSON content from the given <see cref="Stream"/> using specified <see cref="Encoding"/> type.
        /// </summary>
        /// <param name="stream">Required, specify the readable <see cref="Stream"/> that currently is being streaming the JSON content to read.</param>
        /// <param name="encoding">Optional, the <see cref="Encoding"/> object that should be used to decode bytes data into the desired characters.</param>
        /// <param name="bufferSize">Optional, the <see cref="Int32"/> value as the number of bytes to read from <paramref name="stream"/> in each reading cycles.</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that can be used to read the JSON contents from the given <see cref="Stream"/> buffer.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not readable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextReader ReadJson(this Stream stream, Encoding encoding, Int32 bufferSize) => ReadJson(stream, encoding, bufferSize, false);

        /// <summary>
        /// Create a new <see cref="JsonTextReader"/> that reading JSON content from the given <see cref="Stream"/> using specified <see cref="Encoding"/> type.
        /// </summary>
        /// <param name="stream">Required, specify the readable <see cref="Stream"/> that currently is being streaming the JSON content to read.</param>
        /// <param name="encoding">Optional, the <see cref="Encoding"/> object that should be used to decode bytes data into the desired characters.</param>
        /// <param name="bufferSize">Optional, the <see cref="Int32"/> value as the number of bytes to read from <paramref name="stream"/> in each reading cycles.</param>
        /// <param name="disposable">Optional, <see langword="true"/> to dispose the <paramref name="stream"/> just after the <see cref="JsonTextReader"/> disposed; otherwise, set with <see langword="false"/> (default).</param>
        /// <returns>A new instance of the <see cref="JsonTextReader"/> class that can be used to read the JSON contents from the given <see cref="Stream"/> buffer.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not readable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextReader ReadJson(this Stream stream, Encoding encoding, Int32 bufferSize, Boolean disposable)
        {
            var reader = new StreamReader(stream, encoding ?? Encoding.UTF8, true, bufferSize < 1 ? 8192 : bufferSize < 8 ? 8 : bufferSize, !disposable);
            return new JsonTextReader(reader)
            {
                ArrayPool = JsonCharsPool.Instance,
                CloseInput = true,
                Culture = JsonDefault.DefaultCultureInfo,
                DateParseHandling = DateParseHandling.DateTime,
                FloatParseHandling = FloatParseHandling.Double,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                SupportMultipleContent = true
            };
        }

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="StringBuilder"/> object using default preferences.
        /// </summary>
        /// <param name="builder">Required, specify the output <see cref="StringBuilder"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="StringBuilder"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this StringBuilder builder)
            => WriteJson(builder, true, true);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="StringBuilder"/> object using specified preferences.
        /// </summary>
        /// <param name="builder">Required, specify the output <see cref="StringBuilder"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="StringBuilder"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this StringBuilder builder, Boolean pretified)
            => WriteJson(builder, pretified, true);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="StringBuilder"/> object using specified preferences.
        /// </summary>
        /// <param name="builder">Required, specify the output <see cref="StringBuilder"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="StringBuilder"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this StringBuilder builder, Boolean pretified, Boolean escaping)
        {
            var writer = new StringWriter(builder, JsonDefault.DefaultCultureInfo) { NewLine = Environment.NewLine };
            return new JsonTextWriter(writer)
            {
                ArrayPool = JsonCharsPool.Instance,
                CloseOutput = true,
                Culture = JsonDefault.DefaultCultureInfo,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                Formatting = pretified ? Formatting.Indented : Formatting.None,
                Indentation = pretified ? 4 : 0,
                StringEscapeHandling = escaping ? StringEscapeHandling.EscapeNonAscii : StringEscapeHandling.Default
            };
        }

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="TextWriter"/> object using default preferences.
        /// </summary>
        /// <param name="writer">Required, specify the output <see cref="TextWriter"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="TextWriter"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="writer"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this TextWriter writer)
            => WriteJson(writer, true, true, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="TextWriter"/> object using specified preferences.
        /// </summary>
        /// <param name="writer">Required, specify the output <see cref="TextWriter"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="TextWriter"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="writer"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this TextWriter writer, Boolean pretified)
            => WriteJson(writer, pretified, true, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="TextWriter"/> object using specified preferences.
        /// </summary>
        /// <param name="writer">Required, specify the output <see cref="TextWriter"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="TextWriter"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="writer"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this TextWriter writer, Boolean pretified, Boolean escaping)
            => WriteJson(writer, pretified, escaping, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="TextWriter"/> object using specified preferences.
        /// </summary>
        /// <param name="writer">Required, specify the output <see cref="TextWriter"/> that should writen by the JSON contents starting at the current position, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <param name="disposable">Optional, set with <see langword="true"/> to dispose the <see cref="Stream"/> just after the <see cref="JsonTextWriter"/> disposed; otherwise, <see langword="false"/> (default option).</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="TextWriter"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="writer"/> is passed with <see langword="null"/> reference.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this TextWriter writer, Boolean pretified, Boolean escaping, Boolean disposable) => new(writer)
        {
            ArrayPool = JsonCharsPool.Instance,
            CloseOutput = disposable,
            Culture = JsonDefault.DefaultCultureInfo,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            FloatFormatHandling = FloatFormatHandling.DefaultValue,
            Formatting = pretified ? Formatting.Indented : Formatting.None,
            Indentation = pretified ? 4 : 0,
            StringEscapeHandling = escaping ? StringEscapeHandling.EscapeNonAscii : StringEscapeHandling.Default
        };

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="Stream"/> object using default preferences.
        /// </summary>
        /// <param name="stream">Required, the output writeable <see cref="Stream"/> that should writen by the JSON contents starting at the current <see cref="Stream.Position"/>, cannot <see langword="null"/>.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not writeable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this Stream stream)
            => WriteJson(stream, true, true, null, 0, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="Stream"/> object using specified preferences.
        /// </summary>
        /// <param name="stream">Required, the output writeable <see cref="Stream"/> that should writen by the JSON contents starting at the current <see cref="Stream.Position"/>, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not writeable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this Stream stream, Boolean pretified)
            => WriteJson(stream, pretified, true, null, 0, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="Stream"/> object using specified preferences.
        /// </summary>
        /// <param name="stream">Required, the output writeable <see cref="Stream"/> that should writen by the JSON contents starting at the current <see cref="Stream.Position"/>, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not writeable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this Stream stream, Boolean pretified, Boolean escaping)
            => WriteJson(stream, pretified, escaping, null, 0, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="Stream"/> object using specified preferences.
        /// </summary>
        /// <param name="stream">Required, the output writeable <see cref="Stream"/> that should writen by the JSON contents starting at the current <see cref="Stream.Position"/>, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <param name="encoding">Optional, set with <see cref="Encoding"/> object that should be used to encode all writen characters into the bytes data, the default is <see cref="UTF8Encoding"/>.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not writeable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this Stream stream, Boolean pretified, Boolean escaping, Encoding encoding)
            => WriteJson(stream, pretified, escaping, encoding, 0, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="Stream"/> object using specified preferences.
        /// </summary>
        /// <param name="stream">Required, the output writeable <see cref="Stream"/> that should writen by the JSON contents starting at the current <see cref="Stream.Position"/>, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <param name="encoding">Optional, set with <see cref="Encoding"/> object that should be used to encode all writen characters into the bytes data, the default is <see cref="UTF8Encoding"/>.</param>
        /// <param name="bufferSize">Optional, the number of bytes to cache in the memory before it writen to the underlying <see cref="Stream"/> output, the default is 8KB/cycles.</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not writeable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this Stream stream, Boolean pretified, Boolean escaping, Encoding encoding, Int32 bufferSize)
            => WriteJson(stream, pretified, escaping, encoding, bufferSize, false);

        /// <summary>
        /// Create a new <see cref="JsonTextWriter"/> that supported to write the JSON contents into the underlying <see cref="Stream"/> object using specified preferences.
        /// </summary>
        /// <param name="stream">Required, the output writeable <see cref="Stream"/> that should writen by the JSON contents starting at the current <see cref="Stream.Position"/>, cannot <see langword="null"/>.</param>
        /// <param name="pretified">Optional, set with <see langword="true"/> (default) to write indented (pretifed) JSON format; otherwise, set with <see langword="false"/> to write minified JSON.</param>
        /// <param name="escaping">Optional, set with <see langword="true"/> (default) to escape all of non ASCII characters; otherwise, <see langword="false"/> to just escape basic control characters.</param>
        /// <param name="encoding">Optional, set with <see cref="Encoding"/> object that should be used to encode all writen characters into the bytes data, the default is <see cref="UTF8Encoding"/>.</param>
        /// <param name="bufferSize">Optional, the number of bytes to cache in the memory before it writen to the underlying <see cref="Stream"/> output, the default is 8KB/cycles.</param>
        /// <param name="disposable">Optional, set with <see langword="true"/> to dispose the <see cref="Stream"/> just after the <see cref="JsonTextWriter"/> disposed; otherwise, <see langword="false"/> (default option).</param>
        /// <returns>A new instance of the <see cref="JsonTextWriter"/> that can be used to write custom JSON output into the underlying <see cref="Stream"/>, eg. JSON objects or arrays.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is passed with <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> is not writeable.</exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonTextWriter WriteJson(this Stream stream, Boolean pretified, Boolean escaping, Encoding encoding, Int32 bufferSize, Boolean disposable)
        {
            var writer = new StreamWriter(stream, encoding ?? JsonDefault.CreateEncoding(), bufferSize < 1 ? 8192 : bufferSize < 8 ? 8 : bufferSize, !disposable);
            return new JsonTextWriter(writer)
            {
                ArrayPool = JsonCharsPool.Instance,
                CloseOutput = true,
                Culture = JsonDefault.DefaultCultureInfo,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                Formatting = pretified ? Formatting.Indented : Formatting.None,
                Indentation = pretified ? 4 : 0,
                StringEscapeHandling = escaping ? StringEscapeHandling.EscapeNonAscii : StringEscapeHandling.Default
            };
        }

        private static JsonConverter FindConverter<T>(JsonConverterCollection converters, Boolean readMode)
        {
            var count = converters is null ? 0 : converters.Count;
            if (count != 0)
            {
                var index = 0;
                var objectType = typeof(T);
                if (readMode)
                {
                    while (count-- > 0)
                    {
                        if (converters[index++] is JsonConverter converter && converter.CanRead && (converter is JsonConverter<T> || converter.CanConvert(objectType)))
                            return converter;
                    }
                }
                else
                {
                    while (count-- > 0)
                    {
                        if (converters[index++] is JsonConverter converter && converter.CanWrite && (converter is JsonConverter<T> || converter.CanConvert(objectType)))
                            return converter;
                    }
                }
            }
            return null;
        }

        private static JsonConverter FindConverter(Type objectType, JsonConverterCollection converters, Boolean readMode)
        {
            var count = converters is null ? 0 : converters.Count;
            if (count != 0)
            {
                var index = 0;
                if (readMode)
                {
                    while (count-- > 0)
                    {
                        if (converters[index++] is JsonConverter converter && converter.CanRead && converter.CanConvert(objectType))
                        {
                            return converter;
                        }
                    }
                }
                else
                {
                    while (count-- > 0)
                    {
                        if (converters[index++] is JsonConverter converter && converter.CanWrite && converter.CanConvert(objectType))
                        {
                            return converter;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Read the object class of type <typeparamref name="T"/> from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.</typeparam>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <returns>If succeeds, an instance of deserialized <typeparamref name="T"/> object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing <typeparamref name="T"/> object is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static T ReadClass<T>(this JsonReader reader) where T : class => ReadClass<T>(reader, false, null);

        /// <summary>
        /// Read the object class of type <typeparamref name="T"/> from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.</typeparam>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="serializer">Optional, the <see cref="JsonSerializer"/> that used to deserialize the object, or <see langword="null"/> to use default.</param>
        /// <returns>If succeeds, an instance of deserialized <typeparamref name="T"/> object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing <typeparamref name="T"/> object is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static T ReadClass<T>(this JsonReader reader, JsonSerializer serializer) where T : class => ReadClass<T>(reader, false, serializer);

        /// <summary>
        /// Read the object class of type <typeparamref name="T"/> from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.</typeparam>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="silent">Optional, set <see langword="true"/> to suppress all error and return <see langword="null"/> on fail, -or <see langword="false"/> otherwise.<br/>If this parameter is set with <see langword="false"/> (default), a <see cref="JsonException"/> will thrown on failed.</param>
        /// <returns>If succeeds, an instance of deserialized <typeparamref name="T"/> object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if the <paramref name="silent"/> parameter is <see langword="false"/> and at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing <typeparamref name="T"/> object is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static T ReadClass<T>(this JsonReader reader, Boolean silent) where T : class => ReadClass<T>(reader, silent, null);

        /// <summary>
        /// Read the object class of type <typeparamref name="T"/> from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.</typeparam>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="silent">Optional, set <see langword="true"/> to suppress all error and return <see langword="null"/> on fail, -or <see langword="false"/> otherwise.<br/>If this parameter is set with <see langword="false"/> (default), a <see cref="JsonException"/> will thrown on failed.</param>
        /// <param name="serializer">Optional, the <see cref="JsonSerializer"/> that used to deserialize the object, or <see langword="null"/> to use default.</param>
        /// <returns>If succeeds, an instance of deserialized <typeparamref name="T"/> object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if the <paramref name="silent"/> parameter is <see langword="false"/> and at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing <typeparamref name="T"/> object is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static T ReadClass<T>(this JsonReader reader, Boolean silent, JsonSerializer serializer) where T : class
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader), "The JSON reader to read must not be null.");
            if ((reader.TokenType == JsonToken.PropertyName || reader.TokenType == JsonToken.None) && !reader.Read())
            {
                if (silent) return null;
                throw new JsonException($"Cannot parse the object of type {typeof(T)} from unexpected JSON reader state.");
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
            {
                if (serializer is null) serializer = JsonDefault.DefaultJsonPretty;
                if (silent)
                {
                    try
                    {
                        if (FindConverter<T>(serializer.Converters, true) is JsonConverter converter)
                        {
                            if (converter is JsonConverter<T> casting)
                            {
                                return casting.ReadJson(reader, typeof(T), null, false, serializer);
                            }
                            else
                            {
                                return (T)converter.ReadJson(reader, typeof(T), null, serializer);
                            }
                        }
                        return serializer.Deserialize<T>(reader);
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    if (FindConverter<T>(serializer.Converters, true) is JsonConverter converter)
                    {
                        if (converter is JsonConverter<T> casting)
                        {
                            return casting.ReadJson(reader, typeof(T), null, false, serializer);
                        }
                        else
                        {
                            return (T)converter.ReadJson(reader, typeof(T), null, serializer);
                        }
                    }
                    return serializer.Deserialize<T>(reader);
                }
            }
            else
            {
                if (silent) return null;
                throw new JsonException($"Cannot read the object of type {typeof(T)}. Invalid JSON token: {reader.TokenType}");
            }
        }

        /// <summary>
        /// Read the JSON token from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <returns>If succeeds, an instance of deserialized object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing the given object type is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static JToken ReadObject(this JsonReader reader)
            => (JToken)ReadObject(reader, null, false, null);

        /// <summary>
        /// Read the JSON token from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="silent">Optional, set <see langword="true"/> to suppress all error and return <see langword="null"/> on fail, -or <see langword="false"/> otherwise.<br/>If this parameter is set with <see langword="false"/> (default), a <see cref="JsonException"/> will thrown on failed.</param>
        /// <returns>If succeeds, an instance of deserialized object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing the given object type is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static JToken ReadObject(this JsonReader reader, Boolean silent)
            => (JToken)ReadObject(reader, null, silent, null);

        /// <summary>
        /// Read the object of specified type from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="objType">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.<br/>If the <paramref name="objType"/> is <see langword="null"/> then an instance of the <see cref="JToken"/> will used as result type, eg. <see cref="JObject"/>, etc.</param>
        /// <returns>If succeeds, an instance of deserialized object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing the given object type is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static Object ReadObject(this JsonReader reader, Type objType)
            => ReadObject(reader, objType, false, null);

        /// <summary>
        /// Read the object of specified type from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="objType">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.<br/>If the <paramref name="objType"/> is <see langword="null"/> then an instance of the <see cref="JToken"/> will used as result type, eg. <see cref="JObject"/>, etc.</param>
        /// <param name="serializer">Optional, the <see cref="JsonSerializer"/> that used to deserialize the object, or <see langword="null"/> to use default.</param>
        /// <returns>If succeeds, an instance of deserialized object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing the given object type is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static Object ReadObject(this JsonReader reader, Type objType, JsonSerializer serializer)
            => ReadObject(reader, objType, false, serializer);

        /// <summary>
        /// Read the object of specified type from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="objType">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.<br/>If the <paramref name="objType"/> is <see langword="null"/> then an instance of the <see cref="JToken"/> will used as result type, eg. <see cref="JObject"/>, etc.</param>
        /// <param name="silent">Optional, set <see langword="true"/> to suppress all error and return <see langword="null"/> on fail, -or <see langword="false"/> otherwise.<br/>If this parameter is set with <see langword="false"/> (default), a <see cref="JsonException"/> will thrown on failed.</param>
        /// <returns>If succeeds, an instance of deserialized object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if the <paramref name="silent"/> parameter is <see langword="false"/> and at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing the given object type is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static Object ReadObject(this JsonReader reader, Type objType, Boolean silent)
            => ReadObject(reader, objType, silent, null);

        /// <summary>
        /// Read the object of specified type from the <see cref="JsonReader"/> starting at the current <see cref="JsonReader.Path"/>.
        /// </summary>
        /// <param name="reader">Required, set the <see cref="JsonReader"/> that being used to read the streamed JSON contents.</param>
        /// <param name="objType">The <see cref="Type"/> of a <see langword="class"/> that supposed to be located at the current <see cref="JsonReader.Path"/> to deserialize.<br/>If the <paramref name="objType"/> is <see langword="null"/> then an instance of the <see cref="JToken"/> will used as result type, eg. <see cref="JObject"/>, etc.</param>
        /// <param name="silent">Optional, set <see langword="true"/> to suppress all error and return <see langword="null"/> on fail, -or <see langword="false"/> otherwise.<br/>If this parameter is set with <see langword="false"/> (default), a <see cref="JsonException"/> will thrown on failed.</param>
        /// <param name="serializer">Optional, the <see cref="JsonSerializer"/> that used to deserialize the object, or <see langword="null"/> to use default.</param>
        /// <returns>If succeeds, an instance of deserialized object will returned; otherwise, <see langword="null"/> or throw exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is passed with <see langword="null"/>.</exception>
        /// <exception cref="JsonException">This exception are thrown if the <paramref name="silent"/> parameter is <see langword="false"/> and at least one of the following conditions is happened:<br/>
        /// <list type="bullet">
        ///     <item>If the <see cref="JsonReader.TokenType"/> is not <see cref="JsonToken.StartObject"/> or <see cref="JsonToken.Null"/>;</item>
        ///     <item>If the <see cref="JsonReader.TokenType"/> is <see cref="JsonToken.PropertyName"/> and there is no more JSON token afterwards.</item>
        ///     <item>If deserializing the given object type is failed, or some of requried property is missing, or some of values is cannot deserialized.</item>
        /// </list>
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public static Object ReadObject(this JsonReader reader, Type objType, Boolean silent, JsonSerializer serializer)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader), "The JSON reader to read must not be null.");
            if ((reader.TokenType == JsonToken.PropertyName || reader.TokenType == JsonToken.None) && !reader.Read())
            {
                if (silent) return null;
                throw new JsonException($"Cannot read the object {(objType is null ? "" : $"of type {objType}")}. Invalid JSON token: {reader.TokenType}");
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                if (serializer is null) serializer = JsonDefault.DefaultJsonPretty;
                if (silent)
                {
                    if (objType is null)
                    {
                        var result = new JObject();
                        try
                        {
                            JValue jnull = default, jundef = default;
                            JsonLoadSettings ldconf = null;
                            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                            {
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    var property = Convert.ToString(reader.Value, reader.Culture);
                                    if (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.Null)
                                            result.Add(property, (jnull ??= JValue.CreateNull()));
                                        else if (reader.TokenType == JsonToken.Undefined || reader.TokenType == JsonToken.None)
                                            result.Add(property, (jundef ??= JValue.CreateUndefined()));
                                        else
                                            result.Add(property, JToken.Load(reader, (ldconf ??= new JsonLoadSettings { CommentHandling = CommentHandling.Load, LineInfoHandling = LineInfoHandling.Ignore, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace })));
                                    }
                                }
                            }
                        }
                        catch { }
                        return new JObject();
                    }
                    else
                    {
                        try
                        {
                            if (FindConverter(objType, serializer.Converters, true) is JsonConverter converter)
                            {
                                return converter.ReadJson(reader, objType, null, serializer);
                            }
                            return serializer.Deserialize(reader, objType);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
                else if (objType is null)
                {
                    var result = new JObject();
                    JValue jnull = default, jundef = default;
                    JsonLoadSettings ldconf = null;
                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            var property = Convert.ToString(reader.Value, reader.Culture);
                            if (reader.Read())
                            {
                                if (reader.TokenType == JsonToken.Null)
                                    result.Add(property, (jnull ??= JValue.CreateNull()));
                                else if (reader.TokenType == JsonToken.Undefined || reader.TokenType == JsonToken.None)
                                    result.Add(property, (jundef ??= JValue.CreateUndefined()));
                                else
                                    result.Add(property, JToken.Load(reader, (ldconf ??= new JsonLoadSettings { CommentHandling = CommentHandling.Load, LineInfoHandling = LineInfoHandling.Ignore, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace })));
                            }
                        }
                    }
                    return result;
                }
                else
                {
                    if (FindConverter(objType, serializer.Converters, true) is JsonConverter converter)
                    {
                        return converter.ReadJson(reader, objType, null, serializer);
                    }
                    return serializer.Deserialize(reader, objType);
                }
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                if (silent)
                {
                    try
                    {
                        return objType is null
                            ? JArray.Load(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Load, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace, LineInfoHandling = LineInfoHandling.Ignore })
                            : (serializer ?? JsonDefault.DefaultJsonPretty).Deserialize(reader, objType);
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return objType is null
                           ? JArray.Load(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Load, DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace, LineInfoHandling = LineInfoHandling.Ignore })
                           : (serializer ?? JsonDefault.DefaultJsonPretty).Deserialize(reader, objType);
                }
            }
            else
            {
                if (serializer is null) serializer = JsonDefault.DefaultJsonPretty;
                return objType is null ? serializer.Deserialize(reader) : serializer.Deserialize(reader, objType);
            }
        }

        /// <summary>
        /// Copy all of <see cref="JsonConverter"/> that contained in the current <see cref="JsonConverterCollection"/> into a new array.
        /// </summary>
        /// <param name="collection">The instance of <see cref="JsonConverterCollection"/> to copy all of the <see cref="JsonConverter"/> elements.</param>
        /// <returns>The <see cref="Array"/> of <see cref="JsonConverter"/> elements that copied from the given <see cref="JsonConverterCollection"/> instance.</returns>
        /// <filterpriority>2</filterpriority>
        public static JsonConverter[] ToArray(this JsonConverterCollection collection)
        {
            var count = collection is null ? 0 : collection.Count;
            if (count != 0)
            {
                var array = new JsonConverter[count];
                for (var i = 0; i < array.Length; array[i] = collection[i++]) ;
                return array;
            }
            return Array.Empty<JsonConverter>();
        }
    }
}