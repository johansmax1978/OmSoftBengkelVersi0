namespace OMSOFT.Bengkel.Controllers
{
    using Newtonsoft.Json;
    using OMSOFT.Bengkel.Models;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract partial class BaseController
    {
        private static partial class IOModel<TModel> where TModel : class
        {
            private static TModel LoadModelCore(Object state)
            {
                if (state is Stream stream)
                {
                    using var jreader = stream.ReadJson();
                    return DefaultJSON.Deserialize<TModel>(jreader);
                }
                else if (state is TextReader reader)
                {
                    using var jreader = reader.ReadJson();
                    return DefaultJSON.Deserialize<TModel>(jreader);
                }
                else if (state is JsonReader jreader)
                {
                    return DefaultJSON.Deserialize<TModel>(jreader);
                }
                else
                {
                    throw new ArgumentException("Invalid \"state\" type.");
                }

            }

            private static void SaveModelCore(Object state)
            {
                if (state is Tuple<Stream, TModel> mode1)
                {
                    using var writer = mode1.Item1.WriteJson();
                    DefaultJSON.Serialize(writer, mode1.Item2, mode1.Item2?.GetType() ?? typeof(TModel));
                }
                else if (state is Tuple<TextWriter, TModel> mode2)
                {
                    using var writer = mode2.Item1.WriteJson();
                    DefaultJSON.Serialize(writer, mode2.Item2, mode2.Item2?.GetType() ?? typeof(TModel));
                }
                else if (state is Tuple<JsonWriter, TModel> mode3)
                {
                    DefaultJSON.Serialize(mode3.Item1, mode3.Item2, mode3.Item2?.GetType() ?? typeof(TModel));
                }
                else
                {
                    throw new ArgumentException("Invalid state.");
                }
            }

            private static readonly Func<Object, TModel> LoadModelFunc;
            private static readonly Action<Object> SaveModelFunc;

            static IOModel()
            {
                LoadModelFunc = LoadModelCore;
                SaveModelFunc = SaveModelCore;
                RuntimeHelpers.PrepareDelegate(LoadModelFunc);
                RuntimeHelpers.PrepareDelegate(SaveModelFunc);
            }

            public static Task<TModel> LoadModel(Stream stream)
                => Task.Factory.StartNew(LoadModelFunc, stream, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task<TModel> LoadModel(TextReader reader)
                => Task.Factory.StartNew(LoadModelFunc, reader, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task<TModel> LoadModel(JsonReader jreader)
                => Task.Factory.StartNew(LoadModelFunc, jreader, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task SaveModel(Stream stream, TModel model)
                => Task.Factory.StartNew(SaveModelFunc, Tuple.Create(stream, model), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task SaveModel(TextWriter writer, TModel model)
                => Task.Factory.StartNew(SaveModelFunc, Tuple.Create(writer, model), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task SaveModel(JsonTextWriter jwriter, TModel model)
                => Task.Factory.StartNew(SaveModelFunc, Tuple.Create(jwriter, model), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private static partial class IOModels<TModel> where TModel : class
        {
            private static TModel[] LoadModelCore(Object state)
            {
                if (state is Stream stream)
                {
                    using var jreader = stream.ReadJson();
                    return DefaultJSON.Deserialize<TModel[]>(jreader);
                }
                else if (state is TextReader reader)
                {
                    using var jreader = reader.ReadJson();
                    return DefaultJSON.Deserialize<TModel[]>(jreader);
                }
                else if (state is JsonReader jreader)
                {
                    return DefaultJSON.Deserialize<TModel[]>(jreader);
                }
                else
                {
                    throw new ArgumentException("Invalid \"state\" type.");
                }

            }

            private static void SaveModelCore(Object state)
            {
                if (state is Tuple<Stream, TModel[]> mode1)
                {
                    using var writer = mode1.Item1.WriteJson();
                    DefaultJSON.Serialize(writer, mode1.Item2, mode1.Item2?.GetType() ?? typeof(TModel[]));
                }
                else if (state is Tuple<TextWriter, TModel[]> mode2)
                {
                    using var writer = mode2.Item1.WriteJson();
                    DefaultJSON.Serialize(writer, mode2.Item2, mode2.Item2?.GetType() ?? typeof(TModel[]));
                }
                else if (state is Tuple<JsonWriter, TModel[]> mode3)
                {
                    DefaultJSON.Serialize(mode3.Item1, mode3.Item2, mode3.Item2?.GetType() ?? typeof(TModel[]));
                }
                else
                {
                    throw new ArgumentException("Invalid state.");
                }
            }

            private static readonly Func<Object, TModel[]> LoadModelFunc;
            private static readonly Action<Object> SaveModelFunc;

            static IOModels()
            {
                LoadModelFunc = LoadModelCore;
                SaveModelFunc = SaveModelCore;
                RuntimeHelpers.PrepareDelegate(LoadModelFunc);
                RuntimeHelpers.PrepareDelegate(SaveModelFunc);
            }

            public static Task<TModel[]> LoadModel(Stream stream)
                => Task.Factory.StartNew(LoadModelFunc, stream, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task<TModel[]> LoadModel(TextReader reader)
                => Task.Factory.StartNew(LoadModelFunc, reader, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task<TModel[]> LoadModel(JsonReader jreader)
                => Task.Factory.StartNew(LoadModelFunc, jreader, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task SaveModel(Stream stream, TModel[] model)
                => Task.Factory.StartNew(SaveModelFunc, Tuple.Create(stream, model), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task SaveModel(TextWriter writer, TModel[] model)
                => Task.Factory.StartNew(SaveModelFunc, Tuple.Create(writer, model), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            public static Task SaveModel(JsonTextWriter jwriter, TModel[] model)
                => Task.Factory.StartNew(SaveModelFunc, Tuple.Create(jwriter, model), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected static HttpClient DefaultClient => JsonDefault.DefaultHttpClient;

        protected static JsonSerializer DefaultJSON => JsonDefault.DefaultJsonPretty;

        protected static String DefaultHost => "http://localhost/api/bengkel/";

        protected static Uri GenerateURI(BaseController controller, String operation)
            => GenerateURI(controller, operation, queries: null);

        protected static Uri GenerateURI(BaseController controller, String operation, params Query[] queries)
            => GenerateURI(controller.RouteName, operation, queries);

        protected static Uri GenerateURI(String controller, String operation)
            => GenerateURI(controller, operation, queries: null);

        protected static Uri GenerateURI(String controller, String operation, params Query[] queries)
        {
            var builder = new StringBuilder(4096);
            _ = builder.Append(DefaultHost).Append(controller);
            if (operation is not null && operation.Length != 0)
                _ = builder.Append('/').Append(operation);
            if (queries is not null && queries.Length != 0)
                _ = builder.Append('/').Append(Query.Format(queries));
            return new(builder.ToString());
        }

        protected abstract String RouteName { get; }

        protected Task<TModel> HttpGetModel<TModel>(Uri requestUri) where TModel : class
            => this.HttpGetModel<TModel>(requestUri, OutputMode.EnableError, CancellationToken.None);

        protected Task<TModel> HttpGetModel<TModel>(Uri requestUri, OutputMode outputMode) where TModel : class
            => this.HttpGetModel<TModel>(requestUri, outputMode, CancellationToken.None);

        protected virtual async Task<TModel> HttpGetModel<TModel>(Uri requestUri, OutputMode outputMode, CancellationToken cancellation) where TModel : class
        {
            using var response = await DefaultClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellation).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                if ((outputMode & OutputMode.EnableError) != OutputMode.AnySituation) _ = response.EnsureSuccessStatusCode();
                if ((outputMode & OutputMode.OnlySuccess) != OutputMode.AnySituation) return null;
            }
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await IOModel<TModel>.LoadModel(stream).ConfigureAwait(false);
        }

        protected virtual Task<TModel> HttpPostModelData<TModel>(Uri requestUri, TModel model) where TModel : class
            => this.HttpPostModelData(requestUri, model, CancellationToken.None);

        protected virtual async Task<TModel> HttpPostModelData<TModel>(Uri requestUri, TModel model, CancellationToken cancellation) where TModel : class
        {
            var builder = new StringBuilder(8192);
            using (var writer = new StringWriter(builder))
            {
                await IOModel<TModel>.SaveModel(writer, model);
            }
            var content = new StringContent(builder.ToString(), Encoding.UTF8, "application/json");
            using var response = await DefaultClient.PostAsync(requestUri, content, CancellationToken.None);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    using var reader = stream.ReadJson();
                    return DefaultJSON.Deserialize<TModel>(reader);
                }
            }
            if (response.ReasonPhrase is String reason)
            {
                throw new Exception(reason);
            }
            return null;
        }

        protected virtual Task<HttpStatusCode> HttpPostModel<TModel>(Uri requestUri, TModel model) where TModel : class
            => this.HttpPostModel(requestUri, model, CancellationToken.None);

        protected virtual async Task<HttpStatusCode> HttpPostModel<TModel>(Uri requestUri, TModel model, CancellationToken cancellation) where TModel : class
        {
            var builder = new StringBuilder(8192);
            using (var writer = new StringWriter(builder))
            {
                await IOModel<TModel>.SaveModel(writer, model);
            }
            var content = new StringContent(builder.ToString(), Encoding.UTF8, "application/json");
            using var response = await DefaultClient.PostAsync(requestUri, content, CancellationToken.None);
            return response.StatusCode;
        }
    }

    [Flags]
    public enum OutputMode
    {
        AnySituation = 0x0,
        OnlySuccess = 0x1,
        EnableError = 0x2
    }

    public readonly struct Query
    {
        public static Query Create(String name, Object value) => new(name, value);

        public static String Format(IEnumerable<Query> queries)
        {
            if (queries is not null)
            {
                using var iterator = queries.GetEnumerator();
                if (iterator.MoveNext())
                {
                    var builder = new StringBuilder(512).Append('?');
                    do
                        _ = builder.Append(iterator.Current.ToString()).Append('&');
                    while (iterator.MoveNext());
                    builder.Length--;
                    if (builder.Length != 0)
                    {
                        return builder.ToString();
                    }
                }
            }
            return "";
        }

        public Query(String name, Object value)
        {
            this.Name = name;
            this.Value = value;
        }
        public readonly String Name;
        public readonly Object Value;

        public override readonly String ToString()
        {
            var value = this.Value;
            var suffix = value is null
                ? (global::System.String)null
                : value is String @string
                ? @string.Length != 0 && (@string = @string.Trim()).Length != 0 ? @string : null
                : value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.CurrentCulture)
                : value is IConvertible convertible
                ? convertible.ToString(CultureInfo.CurrentCulture)
                : value.ToString();
            return suffix is null || suffix.Length == 0 ? this.Name ?? "" : String.Format("{0}={1}", this.Name, suffix);

        }
    }
}
