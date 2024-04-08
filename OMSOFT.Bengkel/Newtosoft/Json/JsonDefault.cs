#if JSON
namespace Newtonsoft.Json
{
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using JsonErrorEventArgs = Serialization.ErrorEventArgs;

    /// <summary>
    /// Represents the default JSON shared variables, this class is not inheritable.
    /// </summary>
    public static partial class JsonDefault
    {
        private static partial class Internals
        {
            [Serializable]
            private sealed partial class SimpleReferenceComparer : IEqualityComparer<Object>, IEqualityComparer
            {
                public static readonly SimpleReferenceComparer Default = new();
                public SimpleReferenceComparer() { }
                public new Boolean Equals(Object x, Object y) => ReferenceEquals(x, y);
                public Int32 GetHashCode(Object obj) => RuntimeHelpers.GetHashCode(obj);
            }

            private static partial class JsonCustomHandlers
            {
                public sealed partial class ErrorHandlerTrace
                {
                    private static String GetTraceFilePath()
                    {
                        var directory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath("dummy.file")), "logs");
                        var datetime = DateTime.Now;
                        var filename = $"json-error-{datetime.Year:D4}{datetime.Month:D2}{datetime.Day:D2}.log";
                        if (!Directory.Exists(directory)) directory = Directory.CreateDirectory(directory).FullName;
                        return Path.Combine(directory, filename);
                    }

                    public readonly Boolean Silent;
                    public readonly FileStream Stream;
                    public readonly StreamWriter Writer;
                    public readonly Object Locking;
                    public readonly StringBuilder Buffer;

                    public ErrorHandlerTrace(Boolean silent = false)
                    {
                        this.Stream = new FileStream(GetTraceFilePath(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, FileOptions.Asynchronous | FileOptions.WriteThrough);
                        this.Writer = new StreamWriter(this.Stream, Encoding.UTF8, 8192, true) { NewLine = Environment.NewLine, AutoFlush = true };
                        this.Locking = new Object();
                        this.Buffer = new StringBuilder(8192);
                        this.Silent = silent;
                    }

                    ~ErrorHandlerTrace()
                    {
                        try
                        {
                            this.Writer.Dispose();
                            this.Stream.Dispose();
                            this.Buffer.Length = 0;
                        }
                        catch { }
                    }

                    private static void WriteFrames(StringBuilder buffer, Exception error, String indent)
                    {
                        var frames = new StackTrace(error, true).GetFrames();
                        for (var i = 0; i < frames.Length; i++)
                        {
                            var frame = frames[i];
                            var fpath = frame.GetFileName();
                            if (!String.IsNullOrEmpty(fpath))
                            {
                                _ = buffer.Append(indent)
                                          //.Append(" at ").Append(frame.GetMethod().GetSignature(false, true, true, true))
                                          .Append(" at ").Append(frame.GetMethod().ToString())
                                          .Append(" in ").Append(fpath)
                                          .Append(" line ").Append(frame.GetFileLineNumber());
                            }
                            if (i < frames.Length - 1) _ = buffer.AppendLine();
                        }
                    }

                    public void Message(JsonErrorEventArgs args, ErrorContext context)
                    {
                        var builder = this.Buffer;
                        context.Handled = this.Silent;
                        if (context.Error is Exception error)
                        {
                            _ = builder.Append('(').Append(error.GetType().Name).Append(") ").Append(error.Message).AppendLine();
                            WriteFrames(builder, error, "");
                            if (error.InnerException is Exception inner)
                            {
                                var indent = "   ";
                                do
                                {
                                    _ = builder.Append(indent).Append('(').Append(inner.GetType().Name).Append(") ").Append(inner.Message).AppendLine();
                                    WriteFrames(builder, inner, indent);
                                    _ = builder.AppendLine();
                                }
                                while ((inner = inner.InnerException) is not null);
                            }
                        }
                        var length = builder.Length;
                        if (length != 0)
                        {
                            while (length > 0 && Char.IsWhiteSpace(builder[length - 1]))
                                length--;
                            builder.Length = length;
                        }
                        _ = builder.AppendLine().AppendLine($"  Date Time: {DateTime.Now:yyyy/MM/dd HH:mm:ss.fffff}");
                        _ = builder.AppendLine().Append("  JSON Path: ").AppendLine(context.Path);
                        if (args.CurrentObject is Object current)
#if HAS_TYPE_EXTENSIONS
                            _ = builder.Append("  Current Object: ").AppendLine(current.GetType().GetName(false, false));
#else
                            _ = builder.Append("  Current Object: ").AppendLine(current.GetType().ToString());
#endif
                        if (context.Member is not null)
                            _ = builder.Append("  Current Member: ").AppendLine(context.Member.ToString());
                        if (context.Handled)
                            _ = builder.AppendLine("  Is Suppressed: true");
                        length = builder.Length;
                        while (length > 0 && Char.IsWhiteSpace(builder[length - 1])) length--;
                        if (length > 0)
                        {
                            this.Writer.WriteLine(builder.ToString(0, length));
                        }
                    }

                    public void Handle(Object sender, JsonErrorEventArgs args)
                    {
                        lock (this.Locking)
                        {
                            if (args is not null)
                            {
                                if (args.ErrorContext is ErrorContext context)
                                {
                                    var builder = this.Buffer;
                                    context.Handled = this.Silent;
                                    if (context.Error is Exception error)
                                    {
                                        _ = builder.Append('(').Append(error.GetType().Name).Append(") ").Append(error.Message).AppendLine();
                                        WriteFrames(builder, error, "");
                                        if (error.InnerException is Exception inner)
                                        {
                                            var indent = "   ";
                                            do
                                            {
                                                _ = builder.Append(indent).Append('(').Append(inner.GetType().Name).Append(") ").Append(inner.Message).AppendLine();
                                                WriteFrames(builder, inner, indent);
                                                _ = builder.AppendLine();
                                            }
                                            while ((inner = inner.InnerException) is not null);
                                        }
                                    }
                                    var length = builder.Length;
                                    if (length != 0)
                                    {
                                        while (length > 0 && Char.IsWhiteSpace(builder[length - 1]))
                                            length--;
                                        builder.Length = length;
                                    }
                                    _ = builder.AppendLine().AppendLine($"  Date Time: {DateTime.Now:yyyy/MM/dd HH:mm:ss.fffff}");
                                    _ = builder.AppendLine().Append("  JSON Path: ").AppendLine(context.Path);
                                    if (args.CurrentObject is Object current)
#if HAS_TYPE_EXTENSIONS
                                        _ = builder.Append("  Current Object: ").AppendLine(current.GetType().GetName(false, false));
#else
                                        _ = builder.Append("  Current Object: ").AppendLine(current.GetType().ToString());
#endif
                                    if (context.Member is not null)
                                        _ = builder.Append("  Current Member: ").AppendLine(context.Member.ToString());
                                    if (context.Handled)
                                        _ = builder.AppendLine("  Is Suppressed: true");
                                    length = builder.Length;
                                    while (length > 0 && Char.IsWhiteSpace(builder[length - 1])) length--;
                                    if (length > 0)
                                    {
                                        this.Writer.WriteLine(builder.ToString(0, length));
                                    }
                                    builder.Length = 0;
                                }
                            }
                        }
                    }

                    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private static readonly ErrorHandlerTrace _DefaultSilent, _DefaultTrace;

                    static ErrorHandlerTrace()
                    {
                        _DefaultSilent = new ErrorHandlerTrace(true);
                        _DefaultTrace = new ErrorHandlerTrace(false);
                    }

                    public static ErrorHandlerTrace DefaultSilent => _DefaultSilent;

                    public static ErrorHandlerTrace DefaultTrace => _DefaultTrace;
                }

                public static readonly EventHandler<JsonErrorEventArgs> ErrorHandlerSilentDefault;

                public static readonly EventHandler<JsonErrorEventArgs> ErrorHandlerTraceDefault;

                public static readonly EventHandler<JsonErrorEventArgs> ErrorHandlerTraceSilentDefault;

                public static readonly EventHandler<JsonErrorEventArgs> ErrorHandlerFallbackDefault;

                static JsonCustomHandlers()
                {
                    ErrorHandlerSilentDefault = ErrorHandlerSilent;
                    ErrorHandlerTraceDefault = ErrorHandlerTrace.DefaultTrace.Handle;
                    ErrorHandlerTraceSilentDefault = ErrorHandlerTrace.DefaultSilent.Handle;
                    ErrorHandlerFallbackDefault = ErrorHandlerFallback;
                }

                public static void ErrorHandlerSilent(Object sender, JsonErrorEventArgs args)
                {
                    if (args?.ErrorContext is ErrorContext context)
                    {
                        context.Handled = true;
                    }
                }

                public static void ErrorHandlerFallback(Object sender, JsonErrorEventArgs args)
                {
                    if (args.ErrorContext is ErrorContext context)
                    {
                        context.Handled = false;
#if TRACE
                        //Trace.WriteLine(context.Error.Message)
#endif
                    }
                }

                public sealed partial class DefaultTraceWriter : ITraceWriter
                {
                    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private static readonly DefaultTraceWriter _DefaultTrace;
                    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private static readonly DefaultTraceWriter _DefaultSilent;

                    static DefaultTraceWriter()
                    {
                        _DefaultTrace = new DefaultTraceWriter(TraceLevel.Verbose);
                        _DefaultSilent = new DefaultTraceWriter(TraceLevel.Info);
                    }

                    public static DefaultTraceWriter DefaultTrace => _DefaultTrace;

                    public static DefaultTraceWriter DefaultSilent => _DefaultSilent;

                    private static String GetTraceFilePath()
                    {
                        var directory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath("dummy.file")), "logs");
                        var datetime = DateTime.Now;
                        var filename = $"json-trace-{datetime.Year:D4}{datetime.Month:D2}{datetime.Day:D2}.log";
                        if (!Directory.Exists(directory)) directory = Directory.CreateDirectory(directory).FullName;
                        return Path.Combine(directory, filename);
                    }

                    public DefaultTraceWriter(TraceLevel filter = TraceLevel.Info)
                    {
                        this.Filter = filter;
                        this.Stream = new FileStream(GetTraceFilePath(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, FileOptions.Asynchronous | FileOptions.WriteThrough);
                        this.Writer = new StreamWriter(this.Stream, Encoding.UTF8, 8192, true) { NewLine = Environment.NewLine, AutoFlush = true };
                        this.Locking = new Object();
                        this.Buffer = new StringBuilder(8192);
                        this.Accept = new HashSet<TraceLevel>(4)
                        {
                            filter
                        };
                    }

                    ~DefaultTraceWriter()
                    {
                        try
                        {
                            this.Writer.Flush();
                            this.Writer.Dispose();
                            this.Stream.Dispose();
                            this.Buffer.Length = 0;
                        }
                        catch { }
                    }

                    public readonly FileStream Stream;
                    public readonly StreamWriter Writer;
                    public readonly Object Locking;
                    public readonly StringBuilder Buffer;
                    public TraceLevel Filter;
                    public readonly HashSet<TraceLevel> Accept;

                    public void Trace(TraceLevel level, String message, Exception ex)
                    {
                        lock (this.Locking)
                        {
                            if (this.Filter != TraceLevel.Off)
                            {
                                if ((this.Filter == TraceLevel.Verbose && this.Accept.Contains(level)) || ((this.Filter == TraceLevel.Error || this.Filter == TraceLevel.Warning) && ex is not null) || this.Filter == level || this.Accept.Contains(level))
                                {
                                    var buffer = this.Buffer;
                                    var hasmsg = message is not null && message.Length != 0 && (message = message.Trim()).Length != 0;
                                    if (hasmsg) _ = buffer.AppendLine(message);
                                    _ = buffer.AppendLine($"  Date Time: {DateTime.Now:yyyy/MM/dd HH:mm:ss.fffff}");
                                    if (ex is not null)
                                    {
                                        _ = buffer.Append("  Exception: (").Append(ex.GetType().Name).Append(") ").Append(ex.Message).AppendLine();
                                        WriteFrames(buffer, ex, "             ");
                                    }
                                    var length = buffer.Length;
                                    while (length > 0 && Char.IsWhiteSpace(buffer[length - 1])) length--;
                                    if (length > 0)
                                    {
                                        this.Writer.WriteLine(buffer.ToString(0, length));
                                    }
                                }
                            }
                        }
                    }

                    private static void WriteFrames(StringBuilder buffer, Exception error, String indent)
                    {
                        var frames = new StackTrace(error, true).GetFrames();
                        for (var i = 0; i < frames.Length; i++)
                        {
                            var frame = frames[i];
                            var fpath = frame.GetFileName();
                            if (!String.IsNullOrEmpty(fpath))
                            {
                                _ = buffer.Append(indent)
#if HAS_TYPE_EXTENSIONS
                                          .Append(" at ").Append(frame.GetMethod().GetSignature(false, true, true, true))
#else
                                          .Append(" at ").Append(frame.GetMethod().ToString())
#endif
                                          .Append(" in ").Append(fpath)
                                          .Append(" line ").Append(frame.GetFileLineNumber());
                            }
                            if (i < frames.Length - 1) _ = buffer.AppendLine();
                        }
                    }

                    TraceLevel ITraceWriter.LevelFilter => this.Filter;
                }
            }

            private static String DefaultCookiesPath(Boolean ensure)
            {
                var folder = Path.Combine(Path.GetTempPath(), "Life", "data");
                if (ensure && !Directory.Exists(folder)) folder = Directory.CreateDirectory(folder).FullName;
                return Path.Combine(folder, "cookies.json");
            }

            private static CookieContainer DefaultCookiesLoad()
            {
                var path = DefaultCookiesPath(false);
                if (!File.Exists(path)) return new CookieContainer();
                try
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, true);
                    using var reader = new StreamReader(stream, Encoding.UTF8, true, 8192, true);
                    using var jreader = new JsonTextReader(reader) { SupportMultipleContent = true, CloseInput = false, Culture = CultureInfo.CurrentCulture, DateParseHandling = DateParseHandling.DateTime, DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind, FloatParseHandling = FloatParseHandling.Double };
                    if (jreader.Read() && jreader.TokenType == JsonToken.StartArray)
                    {
                        _ = CookieConverter.TryReadContainer(jreader, out var container);
                        return container ?? new CookieContainer();

                    }
                    else
                    {
                        stream.SetLength(0);
                        stream.Write(new Byte[] { (Byte)'[', (Byte)']' }, 0, 2);
                    }
                    return new CookieContainer();
                }
                catch
                {
#if !DEBUG
                    if (File.Exists(path))
                    {
                        try { File.Delete(path); }
                        catch { }
                    }
#endif
                    return new CookieContainer();
                }
            }

            private static void DefaultCookiesSave()
            {
                var path = DefaultCookiesPath(true);
                using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, FileOptions.WriteThrough | FileOptions.Asynchronous);
                using var writer = new StreamWriter(stream, Encoding.UTF8, 8192, true);
                using var jwriter = new JsonTextWriter(writer)
                {
                    CloseOutput = false,
                    Culture = CultureInfo.CurrentCulture,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                    Formatting = Formatting.Indented,
                    FloatFormatHandling = FloatFormatHandling.DefaultValue,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                };
                stream.SetLength(0);
                _ = CookieConverter.TryWriteContainer(jwriter, JsonHttpCookies);
            }

            private static void UnregisterEvents()
            {
                var domain = AppDomain.CurrentDomain;
                domain.ProcessExit -= EventOnShutdown;
                domain.UnhandledException -= EventOnShutdown;
#if NETFX && WIN32
                Microsoft.Win32.SystemEvents.SessionEnding -= EventOnShutdown;
                Microsoft.Win32.SystemEvents.PowerModeChanged += EventOnShutdown;
#endif
            }

#if NETFX && WIN32
            private static void EventOnShutdown(Object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
            {
                if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
                {
                    UnregisterEvents();
                    DefaultCookiesSave();
                }
            }

            private static void EventOnShutdown(Object sender, Microsoft.Win32.SessionEndingEventArgs e)
            {
                UnregisterEvents();
                DefaultCookiesSave();
            }
#endif

            private static void EventOnShutdown(Object sender, EventArgs e)
            {
                UnregisterEvents();
                DefaultCookiesSave();
            }

            private static void EventOnShutdown(Object sender, UnhandledExceptionEventArgs e)
            {
                if (e.IsTerminating)
                {
                    UnregisterEvents();
                    DefaultCookiesSave();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Boolean DefaultCertVerifier(HttpRequestMessage msg, System.Security.Cryptography.X509Certificates.X509Certificate2 cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors) => true;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Boolean DefaultCertVerifier(Object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) => true;

            public static HttpClient CreateHttpClient() => CreateHttpClient(null);

            public static HttpClient CreateHttpClient(Uri address)
            {
                var handler = new HttpClientHandler
                {
                    CookieContainer = JsonHttpCookies,
                    AllowAutoRedirect = true,
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
#if NETFX
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
#else
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls13,
#endif
                    MaxConnectionsPerServer = 512,
                    Proxy = null,
                    UseProxy = false,
                    UseCookies = true,
                    ServerCertificateCustomValidationCallback = CertVerify1
                };
                var client = new HttpClient(handler, true);
                var headers = client.DefaultRequestHeaders;
                headers.UserAgent.Clear();
                headers.UserAgent.TryParseAdd($"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.35");
                headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, NoStore = true, NoTransform = true };
                if (address is not null && address.IsAbsoluteUri)
                {
                    client.BaseAddress = new Uri($"{address.Scheme}://{address.Host}/");
                    headers.Host = address.Host;
                    headers.Referrer = new Uri($"{address.Scheme}://{address.Host}/");
                    _ = headers.TryAddWithoutValidation(":origin", $"{address.Scheme}://{address.Host}");
                    _ = headers.TryAddWithoutValidation(":origin", $"{address.Scheme}://{address.Host}");
                    _ = headers.TryAddWithoutValidation(":authority", address.Host);
                }
                headers.Accept.Clear();
                _ = headers.Accept.TryParseAdd("application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                _ = headers.TryAddWithoutValidation("sec-ch-ua", "Microsoft Edge\";v=\"113\", \" Not;A Brand\";v=\"24\", \"Chromium\";v=\"113\"");
                _ = headers.TryAddWithoutValidation("sec-ch-ua-full-version", "113.0.1774.35");
                _ = headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                _ = headers.TryAddWithoutValidation("sec-ch-ua-platform", "Windows");
                _ = headers.TryAddWithoutValidation("sec-ch-ua-platform-version", "10.0.0");
                _ = headers.TryAddWithoutValidation("sec-fetch-dest", "Empty");
                _ = headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                _ = headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                headers.Accept.Clear();
                _ = headers.Accept.TryParseAdd("*/*");
                client.MaxResponseContentBufferSize = 1024 * 1024 * 256;
                return client;
            }

            private sealed partial class ExtendedWebClient : WebClient
            {
                public ExtendedWebClient() : base() { }

                protected override WebRequest GetWebRequest(Uri address)
                {
                    var request = base.GetWebRequest(address);
                    if (request is HttpWebRequest http)
                    {
                        http.CookieContainer = JsonHttpCookies;
                        http.ProtocolVersion = HttpVersion.Version11;
                        http.ServicePoint.ConnectionLimit = 512;
                    }
                    return request;
                }

                protected override WebResponse GetWebResponse(WebRequest request)
                {
                    var response = base.GetWebResponse(request);
                    if (response is HttpWebResponse http)
                    {
                        var cookies = http.Cookies;
                        if (cookies is not null && cookies.Count != 0)
                        {
                            JsonHttpCookies.Add(cookies);
                        }
                    }
                    return response;
                }
            }

            public static WebClient CreateWebClient(Uri address)
            {
                var client = new ExtendedWebClient();
                client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                client.Credentials = null;
                client.Proxy = null;
                client.UseDefaultCredentials = false;
                client.Encoding = Encoding.UTF8;
                var headers = client.Headers;
                if (address is not null && address.IsAbsoluteUri)
                {
                    client.BaseAddress = "{address.Scheme}://{address.Host}/";
                    headers[HttpRequestHeader.Host] = address.Host;
                    headers[HttpRequestHeader.Referer] = $"{address.Scheme}://{address.Host}/";
                    headers[":origin"] = $"{address.Scheme}://{address.Host}";
                    headers[":authority"] = address.Host;
                }
                headers[HttpRequestHeader.Accept] = "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                headers[HttpRequestHeader.UserAgent] = "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                headers["sec-ch-ua"] = "Microsoft Edge\";v=\"113\", \" Not;A Brand\";v=\"24\", \"Chromium\";v=\"113\"";
                headers["sec-ch-ua-full-version"] = "113.0.1774.35";
                headers["sec-ch-ua-mobile"] = "?0";
                headers["sec-ch-ua-platform"] = "Windows";
                headers["sec-ch-ua-platform-version"] = "10.0.0";
                headers["sec-fetch-dest"] = "Empty";
                headers["sec-fetch-mode"] = "cors";
                headers["sec-fetch-site"] = "same-origin";
                return client;
            }

            public static CultureInfo CreateBaseCulture(String name)
            {
                CultureInfo culture;
                if (name is null || name.Length == 0)
                {
                    culture = new CultureInfo(CultureInfo.InvariantCulture.LCID, true);
                }
                else
                {
                    try
                    {
                        culture = new CultureInfo(name);
                    }
                    catch
                    {
                        culture = new CultureInfo(CultureInfo.InvariantCulture.LCID, true);
                    }
                }
                var nf = culture.NumberFormat;
                nf.CurrencyDecimalSeparator = ".";
                nf.NumberDecimalSeparator = ".";
                nf.PercentDecimalSeparator = ".";
                nf.CurrencyGroupSeparator = ",";
                nf.NumberGroupSeparator = ",";
                nf.PercentGroupSeparator = ",";
                return culture;
            }

            public static JsonConverter[] JsonSetConverters() => CachedJsonConverter;

            public static JsonConverter[] JsonSetConverters(Boolean cached) => cached ? CachedJsonConverter : new JsonConverter[]
            {
                JsonSerializableConverter.Default,
                FontConverter.Default,
                ColorConverter.Default,
                SizeConverter.Default,
                ImageConverter.Default,
                CookieConverter.Default,
                CookieCollectionConverter.Default,
                CookieContainerConverter.Default,
                StringBuilderConverter.Default,
                new BinaryConverter(),
                new BinaryConverter(),
                new DataSetConverter(),
                new DataTableConverter(),
                new EntityKeyMemberConverter(),
                new ExpandoObjectConverter(),
                new IsoDateTimeConverter { Culture = JsonDefaultCulture, DateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.RoundtripKind },
                new KeyValuePairConverter(),
                new RegexConverter(),
                new StringEnumConverter(JsonDefaultNaming, false),
                new VersionConverter(),
                new XmlNodeConverter { DeserializeRootElementName = "Document", EncodeSpecialCharacters = true , OmitRootObject = true , WriteArrayAttribute = true },
                JsonSerializableConverter.Default
            };

            public static void JsonSetConverters(IList<JsonConverter> list)
            {
                if (!list.IsReadOnly)
                {
                    foreach (var converter in JsonSetConverters())
                        list.Add(converter);
                }
            }

            public static EventHandler<JsonErrorEventArgs> JsonErrorHandler(JsonOptions options)
               => (options & JsonOptions.Silent) != JsonOptions.Inline
                   ? (options & JsonOptions.Trace) != JsonOptions.Inline
                       ? JsonCustomHandlers.ErrorHandlerTraceSilentDefault
                       : JsonCustomHandlers.ErrorHandlerSilentDefault
                   : (options & JsonOptions.Trace) != JsonOptions.Inline ? JsonCustomHandlers.ErrorHandlerTraceDefault : null;

            public static ITraceWriter JsonTraceWriter(JsonOptions options)
                => (options & JsonOptions.Trace) != JsonOptions.Inline
                    ? (options & JsonOptions.Silent) != JsonOptions.Inline
                        ? JsonCustomHandlers.DefaultTraceWriter.DefaultSilent
                        : JsonCustomHandlers.DefaultTraceWriter.DefaultTrace
                    : null;

            public static JsonSerializerSettings JsonCreateConfig()
                => JsonCreateConfig(JsonOptions.Pretty | JsonOptions.Silent | JsonOptions.Trace);

            public static JsonSerializerSettings JsonCreateConfig(JsonOptions options) => new()
            {
                Formatting = (options & JsonOptions.Pretty) != JsonOptions.Inline ? Formatting.Indented : Formatting.None,
                CheckAdditionalContent = true,
                ConstructorHandling = ConstructorHandling.Default,
                Context = new StreamingContext(StreamingContextStates.Persistence | StreamingContextStates.File | StreamingContextStates.Remoting),
                ContractResolver = JsonDefaultResolver,
                Converters = JsonSetConverters(),
                Culture = JsonDefaultCulture,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DefaultValueHandling = (options & JsonOptions.Simple) != JsonOptions.Inline ? DefaultValueHandling.IgnoreAndPopulate : DefaultValueHandling.Include,
                EqualityComparer = SimpleReferenceComparer.Default,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = (options & JsonOptions.Simple) != JsonOptions.Inline ? NullValueHandling.Ignore : NullValueHandling.Include,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                SerializationBinder = new DefaultSerializationBinder(),
                StringEscapeHandling = (options & JsonOptions.Pretty) != JsonOptions.Inline ? StringEscapeHandling.EscapeHtml : StringEscapeHandling.EscapeNonAscii,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = (options & JsonOptions.Typed) != JsonOptions.Inline ? TypeNameHandling.Objects : TypeNameHandling.Auto,
                MaxDepth = 512,
                Error = JsonErrorHandler(options),
                TraceWriter = JsonTraceWriter(options)
            };

            public static JsonSerializer JsonCreateEngine(JsonOptions options) => JsonSerializer.Create(JsonCreateConfig(options));

            public static JsonSerializer JsonCreatePretty() => JsonCreateEngine(JsonOptions.Pretty);

            public static JsonSerializer JsonCreateMinify() => JsonCreateEngine(JsonOptions.Inline);

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static readonly Func<HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, Boolean> CertVerify1;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static readonly System.Net.Security.RemoteCertificateValidationCallback CertVerify2;

            private static readonly JsonConverter[] CachedJsonConverter;

            static Internals()
            {
                CertVerify1 = DefaultCertVerifier;
                CertVerify2 = DefaultCertVerifier;
                RuntimeHelpers.PrepareDelegate(CertVerify1);
                RuntimeHelpers.PrepareDelegate(CertVerify2);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = CertVerify2;
                if (ServicePointManager.DefaultConnectionLimit < 512) ServicePointManager.DefaultConnectionLimit = 512;
                JsonHttpCookies = DefaultCookiesLoad();
                JsonDefaultCulture = CreateBaseCulture(null);
                JsonDefaultNaming = new DefaultNamingStrategy
                {
                    OverrideSpecifiedNames = true,
                    ProcessDictionaryKeys = true,
                    ProcessExtensionDataNames = true
                };
                JsonDefaultResolver = new DefaultContractResolver
                {
#pragma warning disable
                    DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
#pragma warning restore
                    IgnoreIsSpecifiedMembers = false,
                    IgnoreSerializableAttribute = true,
                    IgnoreSerializableInterface = false,
                    IgnoreShouldSerializeMembers = false,
                    NamingStrategy = JsonDefaultNaming,
                    SerializeCompilerGeneratedMembers = false
                };
                JsonDefaultPretty = new Lazy<JsonSerializer>(JsonCreatePretty, LazyThreadSafetyMode.ExecutionAndPublication);
                JsonDefaultConfig = new Lazy<JsonSerializerSettings>(JsonCreateConfig, LazyThreadSafetyMode.ExecutionAndPublication);
                JsonDefaultMinify = new Lazy<JsonSerializer>(JsonCreateMinify, LazyThreadSafetyMode.ExecutionAndPublication);
                JsonHttpClient = new Lazy<HttpClient>(CreateHttpClient, LazyThreadSafetyMode.ExecutionAndPublication);
                JsonDefaultEncoding = CreateEncoding();
                CachedJsonConverter = JsonSetConverters(false);
            }

            public static readonly CookieContainer JsonHttpCookies;

            public static CultureInfo JsonDefaultCulture;

            public static readonly NamingStrategy JsonDefaultNaming;

            public static readonly IContractResolver JsonDefaultResolver;

            public static readonly Lazy<JsonSerializer> JsonDefaultPretty;

            public static readonly Lazy<JsonSerializer> JsonDefaultMinify;

            public static readonly Lazy<JsonSerializerSettings> JsonDefaultConfig;

            public static readonly Lazy<HttpClient> JsonHttpClient;

            public static readonly UTF8Encoding JsonDefaultEncoding;


        }

        /// <summary>
        /// Create a new <see cref="UTF8Encoding"/> that can be used as default encoding in the JSON serialization.
        /// </summary>
        /// <returns>A new instance of the <see cref="UTF8Encoding"/> class that provides characters and bytes encoder or decoder.</returns>
        public static UTF8Encoding CreateEncoding() => new(false, false);

        /// <summary>
        /// Represents the default <see cref="Encoding"/> for JSON serialization, that is the <see cref="UTF8Encoding"/> without emitting BOM characters.
        /// </summary>
        /// <value>The instance of <see cref="UTF8Encoding"/> that can be used in the JSON serialization. This encoding will not emit the UTF-8 identifier.</value>
        public static UTF8Encoding DefaultEncoding => Internals.JsonDefaultEncoding;

        /// <summary>
        /// Retrieve the default event handler that supported to handle the JSON operational error based on specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">Choose the valid error handler options, such as <see cref="JsonOptions.Silent"/> or <see cref="JsonOptions.Trace"/>.</param>
        /// <returns>An instance of the <see cref="EventHandler{TEventArgs}"/> where <b>TEventArgs</b> is <see cref="JsonErrorEventArgs"/>.</returns>
        public static EventHandler<JsonErrorEventArgs> JsonErrorHandler(JsonOptions options)
            => Internals.JsonErrorHandler(options) ?? Internals.JsonErrorHandler(JsonOptions.Trace);

        /// <summary>
        /// Create a new <see cref="JsonSerializer"/> using default JSON options, where the serializer will generate pretified JSON output.
        /// </summary>
        /// <returns>A new instance of the <see cref="JsonSerializer"/> that has been configured with default JSON options to produce pretified JSON.</returns>
        public static JsonSerializer CreateJsonEngine()
            => Internals.JsonCreatePretty();

        /// <summary>
        /// Create a new <see cref="JsonSerializer"/> using specified JSON <paramref name="options"/> that control the behaviors and output of the JSON serializer.
        /// </summary>
        /// <param name="options">Choose one or more combinations of the <see cref="JsonOptions"/> as the set of desired JSON options to apply in serializer.</param>
        /// <returns>A new instance of the <see cref="JsonSerializer"/> that has been configured with specified JSON <paramref name="options"/> and the default converters.</returns>
        public static JsonSerializer CreateJsonEngine(JsonOptions options)
            => Internals.JsonCreateEngine(options);

        /// <summary>
        /// Create a new <see cref="JsonSerializerSettings"/> using default JSON options, where the settings will instruct to generate pretified JSON output.
        /// </summary>
        /// <returns>A new instance of the <see cref="JsonSerializerSettings"/> that has been configured with default JSON options, it is ready to use by <see cref="JsonSerializer"/>.</returns>
        public static JsonSerializerSettings CreateJsonConfig()
            => Internals.JsonCreateConfig();

        /// <summary>
        /// Create a new <see cref="JsonSerializerSettings"/> using specified JSON <paramref name="options"/> that control the behaviors and output of the JSON serializer.
        /// </summary>
        /// <param name="options">Choose one or more combinations of the <see cref="JsonOptions"/> as the set of desired JSON options to apply in serializer settings.</param>
        /// <returns>A new instance of the <see cref="JsonSerializerSettings"/> that has been configured with specified JSON <paramref name="options"/> and the default converters.</returns>
        public static JsonSerializerSettings CreateJsonConfig(JsonOptions options)
            => Internals.JsonCreateConfig(options);

        /// <summary>
        /// Represents the default <see cref="JsonSerializer"/> that produces pretified JSON output, used to serialize or deserialize .NET objects to or from JSON.
        /// </summary>
        /// <value>An instance of the <see cref="JsonSerializer"/> class that has been configured to generate indented (pretified) JSON output when used on serialization.</value>
        public static JsonSerializer DefaultJsonPretty
            => Internals.JsonDefaultPretty.Value;

        /// <summary>
        /// Represents the default <see cref="JsonSerializer"/> that produces minified JSON output, used to serialize or deserialize .NET objects to or from JSON.
        /// </summary>
        /// <value>An instance of the <see cref="JsonSerializer"/> class that has been configured to generate inline (minified) JSON output when used on serialization.</value>
        public static JsonSerializer DefaultJsonMinify
            => Internals.JsonDefaultMinify.Value;

        /// <summary>
        /// Represents the default <see cref="JsonSerializerSettings"/> that provides sets of JSON serializer options for the current application.
        /// </summary>
        /// <value>A singleton instance of the <see cref="JsonSerializerSettings"/> that containing global JSON serializer settings for current application.</value>
        public static JsonSerializerSettings DefaultJsonConfig
            => Internals.JsonDefaultConfig.Value;

        /// <summary>
        /// Represents the <see cref="CultureInfo"/> that used as default culture informations in the current application.
        /// </summary>
        /// <value>The instance of <see cref="CultureInfo"/> that containing culture-specific informations for current application.</value>
        public static CultureInfo DefaultCultureInfo
        {
            get => Internals.JsonDefaultCulture;
            set
            {
                if (value != Internals.JsonDefaultCulture)
                {
                    if (value is null)
                    {
                        value = Internals.CreateBaseCulture(null);
                    }
                    else if (value.IsReadOnly)
                    {
                        value = new CultureInfo(value.LCID, true);
                    }
                    if (Internals.JsonDefaultCulture is null || !value.Equals(Internals.JsonDefaultCulture))
                    {
                        Internals.JsonDefaultCulture = value;
                        DefaultJsonConfig.Culture = value;
                        DefaultJsonPretty.Culture = value;
                        DefaultJsonMinify.Culture = value;
                    }
                }
            }
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="HttpClient"/> that provides various HTTP request methods for current application.
        /// </summary>
        /// <value>A singleton instance of the <see cref="HttpClient"/> that provides default HTTP request methods. The user should not dispose this <see cref="HttpClient"/>.</value>
        public static HttpClient DefaultHttpClient
            => Internals.JsonHttpClient.Value;

        /// <summary>
        /// Represents the default <see cref="CookieContainer"/> for all http or web clients that created by the <see cref="JsonDefault"/> service methods.<br/>
        /// All of cookies that stored in this container will be persisted when the parent process exited, and loaded on the startup.
        /// </summary>
        /// <value>An instance of the <see cref="CookieContainer"/> that used as default cookie container for most HTTP operations in the JSON serialization.</value>
        public static CookieContainer HttpJsonCookies => Internals.JsonHttpCookies;

        /// <summary>
        /// Create a new <see cref="WebClient"/> instance that supported to perform various web operations, such as downloading or uploading data.
        /// </summary>
        /// <returns>A new instance of the <see cref="WebClient"/> that supporting to perform common web operations, like downloading or uploading data.</returns>
        public static WebClient CreateWebClient() => Internals.CreateWebClient(null);

        /// <summary>
        /// Create a new <see cref="WebClient"/> instance that supported to perform various web operations, such as downloading or uploading data.
        /// </summary>
        /// <param name="address">The <see cref="Uri"/> that represents URL address to reach or connect, this uri will be set as base address. Set with <see langword="null"/> to ignore.</param>
        /// <returns>A new instance of the <see cref="WebClient"/> that supporting to perform common web operations, like downloading or uploading data.</returns>
        public static WebClient CreateWebClient(Uri address) => Internals.CreateWebClient(address);

        /// <summary>
        /// Create a new preconfigured <see cref="HttpClient"/> instance that supported to perform various HTTP request methods.
        /// </summary>
        /// <returns>A new instance of the <see cref="HttpClient"/> that provides various HTTP request methods, such as GET, POST, and so on.</returns>
        public static HttpClient CreateHttpClient() => Internals.CreateHttpClient();

        /// <summary>
        /// Create a new preconfigured <see cref="HttpClient"/> that supported to perform various HTTP request methods to work with specified URL <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The instance of the <see cref="Uri"/> as the absolute URL address of desired server or host. The host name will be used as base address.</param>
        /// <returns>A new instance of the <see cref="HttpClient"/> that provides various HTTP request methods, such as GET, POST, and so on.</returns>
        public static HttpClient CreateHttpClient(Uri address) => Internals.CreateHttpClient(address);

        /// <summary>
        /// Gets the array of the <see cref="JsonConverter"/> objects that used by the default JSON serializer to convert variety .NET types.
        /// </summary>
        /// <param name="cached"><see langword="true"/> to get the cached <see cref="JsonConverter"/> arrays with its elements; otherwise, always create new.</param>
        /// <returns>The <see cref="JsonConverter"/> array that containing one or more converter objects for the default JSON serializer instance.</returns>
        public static JsonConverter[] JsonConverters(Boolean cached = true) => Internals.JsonSetConverters(cached);
    }
}
#endif