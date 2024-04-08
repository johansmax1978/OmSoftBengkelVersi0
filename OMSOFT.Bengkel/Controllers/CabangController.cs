namespace OMSOFT.Bengkel.Controllers
{
    using Newtonsoft.Json;
    using OMSOFT.Bengkel.Models;
    using OMSOFT.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    
    public partial class CabangController : BaseController
    {
        public CabangController() {  }

        protected override String RouteName => "cabang";

        public async Task<Cabang[]> Get()
        {
            var url = GenerateURI(this, null);
            using var response = await DefaultClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            using var content = response.EnsureSuccessStatusCode().Content;
            using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            using var jreader = stream.ReadJson();
            if(await jreader.ReadAsync().ConfigureAwait(false) && jreader.TokenType == JsonToken.StartArray)
            {
                var serializer = DefaultJSON;
                var list = new List<Cabang>(10);
                while(await jreader.ReadAsync().ConfigureAwait(false) && jreader.TokenType != JsonToken.EndArray)
                {
                    if (jreader.TokenType != JsonToken.StartObject) await jreader.SkipAsync().ConfigureAwait(false);
                    var record = serializer.Deserialize<Cabang>(jreader);
                    if (record is not null) list.Add(record);
                }
                return list.ToArray();
            }
            return Array.Empty<Cabang>();
        }
    }
}
