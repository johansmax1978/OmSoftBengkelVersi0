/*
 * GUI control untuk editor alamat dengan wilayah yang dimana:
 * - Input "provinsi", "kota", "kecamatan", "desa", dan "kodepos" terintegrasi satu sama lain.
 * - GUI control ini membutuhkan koneksi internet.
 * 
 * Keterangan:
 * - AlamatControl memiliki sub-classes yang terintegrasi dengan API (Alamat*), ie. AlamatContent, dll.
 * - Gunakan static method "Load*", misal LoadProvinsi daripada load data menggunakan AlamatRequest class.
 * 
 * TO-DO:
 *      1. Buat cache untuk menghindari konsumsi internet yang berlebihan atau tidak terhubung ke internet.
 *      2. Logging harus dibuat pada fungsi "LoadProvinsi", "LoadKota", "LoadKecamatan", dan "LoadDesa"
 *      3. Buat dokumentasi.
 * 
 * Developer: 
 *      1. Rizky Ramadhana IP, ST., MT.
 *      2. ....
 *      3. ....
 * (isi jika turut serta mengembangkan GUI control ini)
 * Copyright (c) Omsoft Teknologi Group
 */
namespace OMSOFT.Bengkel.Custom
{
    using DevExpress.XtraEditors;
    using DevExpress.XtraSplashScreen;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    [DefaultProperty(nameof(Alamat)), TypeConverter(typeof(ExpandableObjectConverter))]
    public partial class AlamatControl
    {

        // PENTING:
        // Ganti APIRoute menjadi property, dan buat URL sesuai dengan setting-an.
        // Saat ini saya masih menggunakan "hard-code". Jadi nanti dalam aplikasi "REAL"
        // APIRoute berikut harus perupa static properties merujuk kedalam pengaturan aplikasi.

        private const String APIRoute = "http://omsoft.id/wilayah/"; // <--- Ganti dengan settings.

        // konstan berikut sesuai dengan format di API server, sebaiknya biarkan tetap konstan.
        private const String APIMethodListProvinsi = "list-provinsi";
        private const String APIMethodListKota = "list-kota";
        private const String APIMethodListKecamatan = "list-kecamatan";
        private const String APIMethodListDesa = "list-desa";

        #region >>> Helper methods: Utility untuk membantu beberapa logic

        private static Boolean HelperSetCursor(JsonReader jreader, JsonToken token)
        {
            while (jreader.TokenType != token && (jreader.TokenType is JsonToken.None or JsonToken.EndObject or JsonToken.EndArray or JsonToken.Undefined or JsonToken.PropertyName))
            {
                if (!jreader.Read())
                {
                    return false;
                }
            }
            return jreader.TokenType == token;
        }

        private static AlamatContent SafeGetSelectedItem(ComboBoxEdit editor) => editor.InvokeRequired
                ? (AlamatContent)editor.Invoke((Func<ComboBoxEdit, AlamatContent>)SafeGetSelectedItem, editor)
                : (AlamatContent)editor.SelectedItem;

        #endregion

        #region >>> Nested Types: Sub classes dari kelas AlamatControl, digunakan untuk komunikasi REST-API

        /// <summary>
        /// Response yang diberikan oleh REST-API terkait dengan pencarian wilayah.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore, ItemRequired = Required.Default), Serializable]
        public sealed partial class AlamatResponse
        {
            public AlamatResponse() { }

            [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 1)]
            public String Method { get; set; }

            [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 2)]
            public Int32 Status { get; set; }

            [JsonProperty("success", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 3)]
            public Boolean Success { get; set; }

            [JsonProperty("stamp", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 4)]
            public DateTime Stamp { get; set; }

            [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 5)]
            public AlamatContent[] Result { get; set; }

            // Gunakan JsonReader daripada JsonConvert.Deserialize atau JsonSerializer.Deserialize.
            // Untuk mempercepat parsing data (menghindari Reflection).
            public static Boolean TryParse(JsonReader jreader, out AlamatResponse response)
            {
                if (HelperSetCursor(jreader, JsonToken.StartObject))
                {
                    AlamatResponse entity = null;
                    while (jreader.Read() && jreader.TokenType != JsonToken.EndObject)
                    {
                        if (jreader.TokenType != JsonToken.PropertyName) jreader.Skip();
                        else
                        {
                            var property = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture) ?? "";
                            if (jreader.Read())
                            {
                                switch (property.ToLowerInvariant())
                                {
                                    case "method":
                                        if (entity is null) entity = new();
                                        entity.Method = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "success":
                                        if (entity is null) entity = new();
                                        entity.Success = Convert.ToBoolean(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "status":
                                        if (entity is null) entity = new();
                                        entity.Status = Convert.ToInt32(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "stamp":
                                        if (entity is null) entity = new();
                                        entity.Stamp = Epoch.ToDateTime(Convert.ToInt64(jreader.Value, CultureInfo.CurrentCulture));
                                        break;
                                    case "result":
                                        if (jreader.TokenType == JsonToken.Null)
                                        {
                                            if (entity is null) entity = new();
                                            entity.Result = Array.Empty<AlamatContent>();
                                        }
                                        else if (jreader.TokenType == JsonToken.StartObject)
                                        {
                                            if (AlamatContent.TryLoadOne(jreader, out var alamat))
                                            {
                                                if (entity is null) entity = new();
                                                entity.Result = new AlamatContent[] { alamat };
                                            }
                                        }
                                        else if (AlamatContent.TryLoad(jreader, out var result))
                                        {
                                            if (entity is null) entity = new();
                                            entity.Result = result;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                response = null;
                                return false;
                            }
                        }
                    }
                    if (entity is not null)
                    {
                        response = entity;
                        return true;
                    }
                }
                response = null;
                return false;
            }
        }

        /// <summary>
        /// Konten dan hasil pencarian untuk Provinsi, Kota, Kecamatan, dan Desa.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore, ItemRequired = Required.Default), Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public sealed partial class AlamatContent
        {
            public AlamatContent()
            {
                this.Method = (AlamatMethod)(-1);
            }

            /*
             * Catatan:
             * Apabila "AlamatContent" digunakan untuk merepresentasikan data "Provinsi" maka:
             * --> Property "Method" harus "AlamatMethod.ListProvinsi"
             * --> Data KecamatanID, KecamatanNama, ..., DesaID, DesaNama, KodePos akan null.
             * 
             * Dan begitu seterusnya mengikuti tingkatan wilayah berikut (terluas ke terendah):
             * -- Provinsi
             * ---- Kota
             * ------ Kecamatan
             * --------- Desa
             */

            [JsonProperty("id_provinsi", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 1)]
            public Int32 ProvinsiID { get; set; }

            [JsonProperty("id_kota", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 2)]
            public Int32 KotaID { get; set; }

            [JsonProperty("id_kecamatan", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 3)]
            public Int32 KecamatanID { get; set; }

            [JsonProperty("id_desa", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 4)]
            public Int32 DesaID { get; set; }

            [JsonProperty("provinsi", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 5)]
            public String Provinsi { get; set; }

            [JsonProperty("kota", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 6)]
            public String Kota { get; set; }

            [JsonProperty("kecamatan", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 7)]
            public String Kecamatan { get; set; }

            [JsonProperty("desa", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 8)]
            public String Desa { get; set; }

            [JsonProperty("kodepos", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default, Order = 8)]
            public String KodePos { get; set; }

            [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default, Order = 9)]
            public AlamatMethod Method { get; set; }

            public override String ToString() => this.ToString(false);

            public String ToString(Boolean fullPath)
            {
                if (!fullPath)
                {
                    switch (this.Method)
                    {
                        case AlamatMethod.ListProvinsi:
                            return this.Provinsi ?? "null";
                        case AlamatMethod.ListKota:
                            return this.Kota ?? "null";
                        case AlamatMethod.ListKecamatan:
                            return this.Kecamatan ?? "null";
                        case AlamatMethod.ListDesa:
                            return this.Desa ?? "null";
                        default:
                            break;
                    }
                }
                var list = new List<String>(4);
                var value = this.Provinsi;
                if (value is not null && value.Length != 0) list.Add(value);
                if ((value = this.Kota) is not null && value.Length != 0) list.Add(value);
                if ((value = this.Kecamatan) is not null && value.Length != 0) list.Add(value);
                if ((value = this.Desa) is not null && value.Length != 0) list.Add(value);
                return list.Count == 0 ? "null" : String.Join("/", list);
            }

            // Gunakan JsonReader daripada JsonConvert.Deserialize atau JsonSerializer.Deserialize.
            // Untuk mempercepat parsing data (menghindari Reflection).

            public static Boolean TryLoadOne(JsonReader jreader, out AlamatContent content)
            {
                if (jreader.TokenType == JsonToken.StartObject)
                {
                    AlamatContent entity = null;
                    while (jreader.Read() && jreader.TokenType != JsonToken.EndObject)
                    {
                        if (jreader.TokenType != JsonToken.PropertyName) jreader.Skip();
                        else
                        {
                            var name = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture) ?? "";
                            if (jreader.Read())
                            {
                                switch (name.ToLowerInvariant())
                                {
                                    case "id_provinsi":
                                        if (entity is null) entity = new();
                                        entity.ProvinsiID = Convert.ToInt32(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "id_kota":
                                        if (entity is null) entity = new();
                                        entity.KotaID = Convert.ToInt32(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "id_kecamatan":
                                        if (entity is null) entity = new();
                                        entity.KecamatanID = Convert.ToInt32(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "id_desa":
                                        if (entity is null) entity = new();
                                        entity.DesaID = Convert.ToInt32(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "provinsi":
                                    case "nama_provinsi":
                                        if (entity is null) entity = new();
                                        entity.Provinsi = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "kota":
                                    case "nama_kota":
                                        if (entity is null) entity = new();
                                        entity.Kota = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "kecamatan":
                                    case "nama_kecamatan":
                                        if (entity is null) entity = new();
                                        entity.Kecamatan = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "desa":
                                    case "nama_desa":
                                        if (entity is null) entity = new();
                                        entity.Desa = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                    case "kodepos":
                                    case "kode_pos":
                                        if (entity is null) entity = new();
                                        entity.KodePos = Convert.ToString(jreader.Value, CultureInfo.CurrentCulture);
                                        break;
                                }
                            }
                            else
                            {
                                content = null;
                                return false;
                            }
                        }
                    }
                    if (entity is not null)
                    {
                        content = entity;
                        return true;
                    }
                }
                content = null;
                return false;
            }

            public static Boolean TryLoad(JsonReader jreader, out AlamatContent[] contents)
            {
                if (HelperSetCursor(jreader, JsonToken.StartArray))
                {
                    List<AlamatContent> buffer = null;
                    while (jreader.Read() && jreader.TokenType != JsonToken.EndArray)
                    {
                        if (jreader.TokenType == JsonToken.StartObject)
                        {
                            if (TryLoadOne(jreader, out var element))
                            {
                                if (buffer is null) buffer = new(8);
                                buffer.Add(element);
                            }
                        }
                        else
                        {
                            jreader.Skip();
                        }
                    }
                    if (buffer is not null)
                    {
                        contents = buffer.ToArray();
                        return true;
                    }
                }
                contents = Array.Empty<AlamatContent>();
                return false;
            }
        }

        /// <summary>
        /// Metoda pencarian wilayah yang akan digunakan oleh REST-API server.
        /// </summary>
        public enum AlamatMethod
        {
            ListProvinsi,
            ListKota,
            ListKecamatan,
            ListDesa
        }

        /// <summary>
        /// Jenis operator boolean yang digunakan oleh server.
        /// </summary>
        public enum AlamatMatchType
        {
            Equal,
            Like
        }

        /// <summary>
        /// Mendukung HTTP-Request menuju REST-API server untuk mengolah data wilayah. Gunakan metoda berikut daripada kelas ini:<br/>
        /// <see cref="LoadProvinsi"/> -- Untuk query data provinsi.<br/>
        /// <see cref="LoadKota(Object)"/> -- Untuk query data kota.<br/>
        /// <see cref="LoadKecamatan(Object, Object)"/> -- Untuk query data kecamatan.<br/>
        /// <see cref="LoadDesa(Object, Object, Object)"/> -- Untuk query data desa/kelurahan.
        /// </summary>
        public sealed partial class AlamatRequest
        {
            // _Method: Metoda yang akan digunakan dalam HTTP-REQUEST ini.
            private readonly AlamatMethod _Method;

            // _*Nama: field untuk mencari berdasarkan nama.
            private String _ProvinsiNama, _KotaNama, _KecamatanNama, _DesaNama, _KodePos;

            // _*ID: field untuk mencari berdasarkan ID.
            private Int32? _ProvinsiID, _KotaID, _KecamatanID, _DesaID;

            // _*Match: AlamatMatchType.Equal untuk menggunakan operator '=' atau Like untuk menggunakan operator "LIKE"
            private AlamatMatchType _ProvinsiMatch, _KotaMatch, _KecamatanMatch, _DesaMatch, _KodePosMatch;

            // _Allow*: Jika TRUE maka filter berdasarkan kriteria tersebut dibolehkan.
            //        : Jika FALSE maka akan melempar InvalidOperationException jika menggunakan filter tersebut.
            //
            // Misal _AllowKota = FALSE, maka FindByKotaID dan FindByKotaNama akan melempar
            // InvalidOperationException apabila digunakan. Hal ini mengikuti cara kerja yang diminta oleh server.
            private readonly Boolean _AllowProvinsi, _AllowKota, _AllowKecamatan, _AllowDesa;

            public AlamatRequest(AlamatMethod method)
            {
                switch (method)
                {
                    case AlamatMethod.ListProvinsi:
                        this._AllowProvinsi = true;
                        break;
                    case AlamatMethod.ListKota:
                        this._AllowKota = true;
                        goto case AlamatMethod.ListProvinsi;
                    case AlamatMethod.ListKecamatan:
                        this._AllowKecamatan = true;
                        goto case AlamatMethod.ListKota;
                    case AlamatMethod.ListDesa:
                        this._AllowDesa = true;
                        goto case AlamatMethod.ListKecamatan;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(method), method.ToString(), "Invalid method to use.");
                }
                this._Method = method;
            }

            public AlamatMethod Method => this._Method;

            public AlamatRequest FindByID(Int32 id) => this._Method switch
            {
                AlamatMethod.ListProvinsi => this.FindByProvinsiID(id),
                AlamatMethod.ListKota => this.FindByKotaID(id),
                AlamatMethod.ListKecamatan => this.FindByKecamatanID(id),
                AlamatMethod.ListDesa => this.FindByDesaID(id),
                _ => this,
            };

            public AlamatRequest FindByNama(String nama) => this._Method switch
            {
                AlamatMethod.ListProvinsi => this.FindByProvinsiNama(nama),
                AlamatMethod.ListKota => this.FindByKotaNama(nama),
                AlamatMethod.ListKecamatan => this.FindByKecamatanNama(nama),
                AlamatMethod.ListDesa => this.FindByDesaNama(nama),
                _ => this
            };

            public AlamatRequest FindByNama(String nama, AlamatMatchType mode) => this._Method switch
            {
                AlamatMethod.ListProvinsi => this.FindByProvinsiNama(nama, mode),
                AlamatMethod.ListKota => this.FindByKotaNama(nama, mode),
                AlamatMethod.ListKecamatan => this.FindByKecamatanNama(nama, mode),
                AlamatMethod.ListDesa => this.FindByDesaNama(nama, mode),
                _ => this
            };

            public AlamatRequest FindByProvinsiID(Int32 id)
            {
                if (!this._AllowProvinsi) throw new InvalidOperationException($"Pencarian berdasarkan provinsi tidak di dukung untuk metode pencarian: {this.Method}");
                this._ProvinsiID = id < 1 ? null : id;
                return this;
            }

            public AlamatRequest FindByProvinsiNama(String nama)
                => this.FindByProvinsiNama(nama, nama is not null && nama.Length != 0 && nama.IndexOf('%') != -1 ? AlamatMatchType.Like : AlamatMatchType.Equal);

            public AlamatRequest FindByProvinsiNama(String nama, AlamatMatchType mode)
            {
                if (!this._AllowProvinsi) throw new InvalidOperationException($"Pencarian berdasarkan provinsi tidak di dukung untuk metode pencarian: {this.Method}");
                if (nama is null) throw new ArgumentNullException(nameof(nama), "Nama provinsi yang akan dicari tidak boleh NULL.");
                this._ProvinsiNama = nama;
                this._ProvinsiMatch = mode;
                return this;
            }

            public AlamatRequest FindByKotaID(Int32 id)
            {
                if (!this._AllowKota) throw new InvalidOperationException($"Pencarian berdasarkan kota tidak di dukung untuk metode pencarian: {this.Method}");
                this._KotaID = id < 1 ? null : id;
                return this;
            }

            public AlamatRequest FindByKotaNama(String nama)
                => this.FindByKotaNama(nama, nama is not null && nama.Length != 0 && nama.IndexOf('%') != -1 ? AlamatMatchType.Like : AlamatMatchType.Equal);

            public AlamatRequest FindByKotaNama(String nama, AlamatMatchType mode)
            {
                if (!this._AllowKota) throw new InvalidOperationException($"Pencarian berdasarkan kota tidak di dukung untuk metode pencarian: {this.Method}");
                if (nama is null) throw new ArgumentNullException(nameof(nama), "Nama kota yang akan dicari tidak boleh NULL.");
                this._KotaNama = nama;
                this._KotaMatch = mode;
                return this;
            }

            public AlamatRequest FindByKecamatanID(Int32 id)
            {
                if (!this._AllowKecamatan) throw new InvalidOperationException($"Pencarian berdasarkan kecamatan tidak di dukung untuk metode pencarian: {this.Method}");
                this._KecamatanID = id < 1 ? null : id;
                return this;
            }

            public AlamatRequest FindByKecamatanNama(String nama)
                => this.FindByKecamatanNama(nama, nama is not null && nama.Length != 0 && nama.IndexOf('%') != -1 ? AlamatMatchType.Like : AlamatMatchType.Equal);

            public AlamatRequest FindByKecamatanNama(String nama, AlamatMatchType mode)
            {
                if (!this._AllowKecamatan) throw new InvalidOperationException($"Pencarian berdasarkan kecamatan tidak di dukung untuk metode pencarian: {this.Method}");
                if (nama is null) throw new ArgumentNullException(nameof(nama), "Nama kecamatan yang akan dicari tidak boleh NULL.");
                this._KecamatanNama = nama;
                this._KecamatanMatch = mode;
                return this;
            }

            public AlamatRequest FindByDesaID(Int32 id)
            {
                if (!this._AllowDesa) throw new InvalidOperationException($"Pencarian berdasarkan desa tidak di dukung untuk metode pencarian: {this.Method}");
                this._DesaID = id < 1 ? null : id;
                return this;
            }

            public AlamatRequest FindByDesaNama(String nama)
                => this.FindByDesaNama(nama, nama is not null && nama.Length != 0 && nama.IndexOf('%') != -1 ? AlamatMatchType.Like : AlamatMatchType.Equal);

            public AlamatRequest FindByDesaNama(String nama, AlamatMatchType mode)
            {
                if (!this._AllowDesa) throw new InvalidOperationException($"Pencarian berdasarkan desa tidak di dukung untuk metode pencarian: {this.Method}");
                if (nama is null) throw new ArgumentNullException(nameof(nama), "Nama desa yang akan dicari tidak boleh NULL.");
                this._DesaNama = nama;
                this._DesaMatch = mode;
                return this;
            }

            public AlamatRequest FindByKodePos(String kodepos)
                => this.FindByKodePos(kodepos, kodepos is not null && kodepos.Length != 0 && kodepos.IndexOf('%') != -1 ? AlamatMatchType.Like : AlamatMatchType.Equal);

            public AlamatRequest FindByKodePos(String kodepos, AlamatMatchType mode)
            {
                if (!this._AllowDesa) throw new InvalidOperationException($"Pencarian berdasarkan desa tidak di dukung untuk metode pencarian: {this.Method}");
                if (kodepos is null) throw new ArgumentNullException(nameof(kodepos), "Kode pos desa yang akan dicari tidak boleh NULL.");
                this._KodePos = kodepos;
                this._KodePosMatch = mode;
                return this;
            }

            // ToString: Format seluruh logics yang ada pada AlamatRequest ini menjadi URL query string.
            // Jangan buat metoda baru! Gunakan ToString supaya mudah jika AlamatRequest
            // direpresentasikan sebagai "object".
            public override String ToString()
            {
                var builder = new StringBuilder(512).Append(APIRoute).Append("?method=").Append(this.Method switch
                {
                    AlamatMethod.ListProvinsi => APIMethodListProvinsi,
                    AlamatMethod.ListKota => APIMethodListKota,
                    AlamatMethod.ListKecamatan => APIMethodListKecamatan,
                    AlamatMethod.ListDesa => APIMethodListDesa,
                    _ => ""
                });
                if (this._AllowProvinsi)
                    AppendQuery(builder, this._Method == AlamatMethod.ListProvinsi, "provinsi", this._ProvinsiID, this._ProvinsiNama, this._ProvinsiMatch);
                if (this._AllowKota)
                    AppendQuery(builder, this._Method == AlamatMethod.ListKota, "kota", this._KotaID, this._KotaNama, this._KotaMatch);
                if (this._AllowKecamatan)
                    AppendQuery(builder, this._Method == AlamatMethod.ListKecamatan, "kecamatan", this._KecamatanID, this._KecamatanNama, this._KecamatanMatch);
                if (this._AllowDesa)
                    AppendDesa(builder, this._Method == AlamatMethod.ListDesa, "desa", this._DesaID, this._DesaNama, this._DesaMatch, this._KodePos, this._KodePosMatch);
                return builder.ToString();
            }

            // SendRequestAsync: Kirim HTTP-GET request (asynchronous) ke server menggunakan logics yang diterapkan.
            public Task<HttpResponseMessage> SendRequestAsync()
                => this.SendRequestAsync(null, CancellationToken.None);

            public Task<HttpResponseMessage> SendRequestAsync(HttpClient client)
                => this.SendRequestAsync(client, CancellationToken.None);

            public Task<HttpResponseMessage> SendRequestAsync(CancellationToken ctoken)
                => this.SendRequestAsync(null, ctoken);

            public Task<HttpResponseMessage> SendRequestAsync(HttpClient client, CancellationToken ctoken)
            {
                if (ctoken.IsCancellationRequested) return Task.FromCanceled<HttpResponseMessage>(ctoken);
                if (client is null) client = JsonDefault.DefaultHttpClient;
                var uri = new Uri(this.ToString(), UriKind.Absolute);
                return client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ctoken);
            }

            // Utility untuk method ToString, edit jika ingin menambahkan field.
            private static void AppendDesa(StringBuilder builder, Boolean direct, String column, Int32? id, String nama, AlamatMatchType mode, String kodepos, AlamatMatchType kodeposMode)
            {
                if (id.HasValue)
                {
                    _ = direct
                        ? builder.Append("&id=").Append(id.Value)
                        : builder.Append('&').Append(column).Append("_id=").Append(id.Value);
                }
                else if (nama is not null && nama.Length != 0)
                {
                    _ = direct
                        ? builder.Append("&nama=").Append(Uri.EscapeDataString(mode == AlamatMatchType.Like ? String.Concat("{0}%", nama) : nama))
                        : builder.Append('&').Append(column).Append("_nama=").Append(Uri.EscapeDataString(mode == AlamatMatchType.Like ? String.Concat("{0}%", nama) : nama));
                }
                else if (kodepos is not null && kodepos.Length != 0)
                {
                    _ = builder.Append(direct ? "&kodepos=" : "&desa_kodepos=").Append(Uri.EscapeDataString(mode == AlamatMatchType.Like ? String.Concat(kodepos, "%") : kodepos));
                }
            }

            private static void AppendQuery(StringBuilder builder, Boolean direct, String column, Int32? id, String nama, AlamatMatchType mode)
            {
                if (id.HasValue)
                {
                    _ = direct
                        ? builder.Append("&id=").Append(id.Value)
                        : builder.Append('&').Append(column).Append("_id=").Append(id.Value);
                }
                else if (nama is not null && nama.Length != 0)
                {
                    _ = direct
                        ? builder.Append("&nama=").Append(Uri.EscapeDataString(mode == AlamatMatchType.Like ? String.Concat("{0}%", nama) : nama))
                        : builder.Append('&').Append(column).Append("_nama=").Append(Uri.EscapeDataString(mode == AlamatMatchType.Like ? String.Concat("{0}%", nama) : nama));
                }
            }
        }

        #endregion

        #region >>> Fungsu Load* (static)

        // Metoda "Load*" adalah untuk mencari data yang berkaitan, misal LoadProvinsi maka digunakan untuk mencari data provinsi.
        // Parameter dari metoda "Load*" bertipe "Object" yang dimana tipe parameter yang didukung adalah:
        // --- AlamatContent class
        // --- String (merepresentasikan nama wilayah tersebut)
        // --- Int32 (mepresentasikan ID dari tabel wilayah tersebut).
        // --- NULL (jika tidak menggunakan filter).

        public static async Task<AlamatContent[]> LoadProvinsi()
        {
            try
            {
                var request = new AlamatRequest(AlamatMethod.ListProvinsi);
                using var response = await request.SendRequestAsync();
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = stream.ReadJson(Encoding.UTF8, 8192);
                    if (AlamatResponse.TryParse(reader, out var output))
                    {
                        // Sukses, tidak perlu logging.
                        return output.Result ?? Array.Empty<AlamatContent>();
                    }
                }
                // TODO: Tambahkan log disini
            }
            catch
            {
                // TODO: Tambahkan log disini
            }
            return Array.Empty<AlamatContent>();
        }

        public static async Task<AlamatContent[]> LoadKota(Object provinsi = null)
        {
            try
            {
                var request = new AlamatRequest(AlamatMethod.ListKota);
                if (provinsi is not null)
                {
                    if (provinsi is AlamatContent provInfo)
                    {
                        if (provInfo.ProvinsiID > 0)
                            _ = request.FindByProvinsiID(provInfo.ProvinsiID);
                        else if (!String.IsNullOrEmpty(provInfo.Provinsi))
                            _ = request.FindByProvinsiNama(provInfo.Provinsi);
                    }
                    else if (provinsi is String provNama)
                        _ = request.FindByProvinsiNama(provNama);
                    else if (provinsi is Int32 provID)
                        _ = request.FindByProvinsiID(provID);
                }
                using var response = await request.SendRequestAsync();
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = stream.ReadJson(Encoding.UTF8, 8192);
                    if (AlamatResponse.TryParse(reader, out var output))
                    {
                        // Sukses, tidak perlu logging.
                        return output.Result ?? Array.Empty<AlamatContent>();
                    }
                }
                // TODO: Tambahkan log disini
            }
            catch
            {
                // TODO: Tambahkan log disini
            }
            return Array.Empty<AlamatContent>();
        }

        public static async Task<AlamatContent[]> LoadKecamatan(Object provinsi = null, Object kota = null)
        {
            try
            {
                var request = new AlamatRequest(AlamatMethod.ListKecamatan);
                if (provinsi is not null)
                {
                    if (provinsi is AlamatContent provInfo)
                    {
                        if (provInfo.ProvinsiID > 0)
                            _ = request.FindByProvinsiID(provInfo.ProvinsiID);
                        else if (!String.IsNullOrEmpty(provInfo.Provinsi))
                            _ = request.FindByProvinsiNama(provInfo.Provinsi);
                    }
                    else if (provinsi is String provNama)
                        _ = request.FindByProvinsiNama(provNama);
                    else if (provinsi is Int32 provID)
                        _ = request.FindByProvinsiID(provID);
                }
                if (kota is not null)
                {
                    if (kota is AlamatContent kotaInfo)
                    {
                        if (kotaInfo.KotaID > 0)
                            _ = request.FindByKotaID(kotaInfo.KotaID);
                        else if (!String.IsNullOrEmpty(kotaInfo.Kota))
                            _ = request.FindByKotaNama(kotaInfo.Kota);
                    }
                    else if (kota is String kotaNama)
                        _ = request.FindByKotaNama(kotaNama);
                    else if (kota is Int32 kotaID)
                        _ = request.FindByKotaID(kotaID);
                }
                using var response = await request.SendRequestAsync();
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = stream.ReadJson(Encoding.UTF8, 8192);
                    if (AlamatResponse.TryParse(reader, out var output))
                    {
                        // Sukses, tidak perlu logging.
                        return output.Result ?? Array.Empty<AlamatContent>();
                    }
                }
                // TODO: Tambahkan log disini
            }
            catch
            {
                // TODO: Tambahkan log disini
            }
            return Array.Empty<AlamatContent>();
        }

        public static async Task<AlamatContent[]> LoadDesa(Object provinsi = null, Object kota = null, Object kecamatan = null)
        {
            try
            {
                var request = new AlamatRequest(AlamatMethod.ListDesa);
                if (provinsi is not null)
                {
                    if (provinsi is AlamatContent provInfo)
                    {
                        if (provInfo.ProvinsiID > 0)
                            _ = request.FindByProvinsiID(provInfo.ProvinsiID);
                        else if (!String.IsNullOrEmpty(provInfo.Provinsi))
                            _ = request.FindByProvinsiNama(provInfo.Provinsi);
                    }
                    else if (provinsi is String provNama)
                        _ = request.FindByProvinsiNama(provNama);
                    else if (provinsi is Int32 provID)
                        _ = request.FindByProvinsiID(provID);
                }
                if (kota is not null)
                {
                    if (kota is AlamatContent kotaInfo)
                    {
                        if (kotaInfo.KotaID > 0)
                            _ = request.FindByKotaID(kotaInfo.KotaID);
                        else if (!String.IsNullOrEmpty(kotaInfo.Kota))
                            _ = request.FindByKotaNama(kotaInfo.Kota);
                    }
                    else if (kota is String kotaNama)
                        _ = request.FindByKotaNama(kotaNama);
                    else if (kota is Int32 kotaID)
                        _ = request.FindByKotaID(kotaID);
                }
                if (kecamatan is not null)
                {
                    if (kecamatan is AlamatContent kecamatanInfo)
                    {
                        if (kecamatanInfo.KecamatanID > 0)
                            _ = request.FindByKecamatanID(kecamatanInfo.KecamatanID);
                        else if (!String.IsNullOrEmpty(kecamatanInfo.Kecamatan))
                            _ = request.FindByKecamatanNama(kecamatanInfo.Kecamatan);
                    }
                    else if (kecamatan is String kecamatanNama)
                        _ = request.FindByKecamatanNama(kecamatanNama);
                    else if (kecamatan is Int32 kecamatanID)
                        _ = request.FindByKecamatanID(kecamatanID);
                }
                using var response = await request.SendRequestAsync();
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = stream.ReadJson(Encoding.UTF8, 8192);
                    if (AlamatResponse.TryParse(reader, out var output))
                    {
                        // Sukses, tidak perlu logging.
                        return output.Result ?? Array.Empty<AlamatContent>();
                    }
                }
                // TODO: Tambahkan log disini
            }
            catch
            {
                // TODO: Tambahkan log disini
            }
            return Array.Empty<AlamatContent>();
        }

        #endregion

        #region >>> Properties: Sebagai publik akses informasi yang diolah dengan GUI ini.

        [Browsable(true), Category("Wilayah")]
        public String Alamat
        {
            get => this.iAlamat.Text;
            set => this.iAlamat.Text = value ?? "";
        }

        [Browsable(true), Category("Wilayah")]
        public AlamatContent Provinsi => SafeGetSelectedItem(this.iProvinsi);

        [Browsable(true), Category("Wilayah")]
        public AlamatContent Kota => SafeGetSelectedItem(this.iKota);

        [Browsable(true), Category("Wilayah")]
        public AlamatContent Kecamatan => SafeGetSelectedItem(this.iKecamatan);

        [Browsable(true), Category("Wilayah")]
        public AlamatContent Desa => SafeGetSelectedItem(this.iDesa);

        #endregion

        #region >>> SplashScreen:  Digunakan untuk menampilkan splash screen ketika loading data dari server

        [Category("SplashScreen")]
        public OverlayTextPainter SplashScreenPainter
        {
            get
            {
                var value = this.m_SplashPainter;
                if (value is null)
                {
                    this.m_SplashPainter = value = new OverlayTextPainter("Mengambil Data")
                    {
                        Font = this.Font,
                        Color = Color.White
                    };
                }
                return value;
            }
        }

        [Category("SplashScreen")]
        public OverlayWindowOptions SplashScreenOptions
        {
            get
            {
                var value = this.m_SplashOptions;
                if (value is null)
                {
                    this.m_SplashOptions = value = new OverlayWindowOptions
                        (fadeIn: true,
                        fadeOut: false,
                        backColor: Color.FromArgb(255, 34, 34, 34),
                        foreColor: Color.White,
                        opacity: 0.85,
                        customPainter: this.SplashScreenPainter,
                        useDirectX: true,
                        disableInput: true,
                        animationType: WaitAnimationType.Line,
                        lineAnimationParameters: new LineAnimationParams(10, 5, 5));
                }
                return value;
            }
        }

        [Browsable(false)]
        public Boolean SplashScreenActive => this.m_SplashHandle is not null;

        public OverlayTextPainter SplashScreenShow()
        {
            if (this.m_SplashHandle is not null)
            {
                return this.SplashScreenPainter;
            }
            var painter = this.SplashScreenPainter;
            var options = this.SplashScreenOptions;
            try
            {
                var handle = SplashScreenManager.ShowOverlayForm((Control)FindForm() ?? this, options);
                this.m_SplashHandle = handle;
            }
            catch { }
            return painter;
        }

        public void SplashScreenClose()
        {
            if (this.m_SplashHandle is IOverlaySplashScreenHandle handle)
            {
                try { SplashScreenManager.CloseOverlayForm(handle); handle.Dispose(); }
                catch { }
                finally { this.m_SplashHandle = null; }
            }
        }

        #endregion

        #region >>> DisplayError: Tampilkan dialog error apabila terjadi disaat runtime.

        public void DisplayError(Exception error, [CallerMemberName] String operation = "")
            => _ = XtraMessageBox.Show(this, error.ToString(), $"OMSOFT Bengkel Error - {operation}", MessageBoxButtons.OK, MessageBoxIcon.Error);

        #endregion

        #region >>> Constructor class

        public AlamatControl()
        {
            this.InitializeComponent();
            this.InitializeRuntime();
        }

        public AlamatControl(IContainer container) : this() => container?.Add(this);

        #endregion

        #region >>> Private Logics: Metoda internal yang membuat GUI ini menjadi "Hidup", sebaiknya tidak perlu diubah jika memungkinkan

        private void InitializeRuntime()
        {
            this.iProvinsi.QueryPopUp += this.OnProvinsiQueryPopup;
            this.iKota.QueryPopUp += this.OnKotaQueryPopup;
            this.iKecamatan.QueryPopUp += this.OnKecamatanQueryPopup;
            this.iDesa.QueryPopUp += this.OnDesaQueryPopup;
            this.iProvinsi.SelectedIndexChanged += this.OnProvinsiSelectedIndexChanged;
            this.iKota.SelectedIndexChanged += this.OnKotaSelectedIndexChanged;
            this.iKecamatan.SelectedIndexChanged += this.OnKecamatanSelectedIndexChanged;
            this.iDesa.SelectedIndexChanged += this.OnDesaSelectedIndexChanged;
            this.ButtonResetInput.HyperlinkClick += this.OnButtonResetInputClick;
            this.iProvinsi.KeyDown += this.OnProvinsiKeyDown;
            this.iKota.KeyDown += this.OnKotaKeyDown;
            this.iKecamatan.KeyDown += this.OnKecamatanKeyDown;
            this.iDesa.KeyDown += this.OnDesaKeyDown;
            this.iKodePos.Properties.ButtonClick += this.KodePosButtonCopyClick;
            this.iProvinsi.Properties.ButtonClick += this.OnHandleButtonCopyClick;
            this.iKota.Properties.ButtonClick += this.OnHandleButtonCopyClick;
            this.iKecamatan.Properties.ButtonClick += this.OnHandleButtonCopyClick;
            this.iDesa.Properties.ButtonClick += this.OnHandleButtonCopyClick;
            if (this.FindForm() is Form form) this.iAlamat.BackColor = form.BackColor;
            else if (this.Parent is Control control) this.iAlamat.BackColor = control.BackColor;
            DevExpress.LookAndFeel.UserLookAndFeel.Default.StyleChanged += this.OnDefaultStyleChanged;
        }

        private void OnDefaultStyleChanged(Object sender, EventArgs e)
        {
            if (this.FindForm() is Form form) this.iAlamat.BackColor = form.BackColor;
            else if (this.Parent is Control control) this.iAlamat.BackColor = control.BackColor;
        }

        private void OnHandleButtonCopyClick(Object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Index == 1 && sender is ComboBoxEdit editor && editor.Text is String text && text.Length != 0)
            {
                Clipboard.SetText(text);
            }
        }

        private void KodePosButtonCopyClick(Object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Index == 0 && this.iKodePos.Text is String text && text.Length != 0)
            {
                if ((ModifierKeys & Keys.Control) != 0)
                    Clipboard.SetText(String.Format("{0}/{1}/{2}/{3} ({4})", this.iProvinsi.Text, this.iKota.Text, this.iKecamatan.Text, this.iDesa.Text, text));
                else
                    Clipboard.SetText(String.Format("Kelurahan {0}, Kecamatan {1}, Kota {2}, Provinsi {3}, {4}", this.iDesa.Text, this.iKecamatan.Text, this.iKota.Text, this.iProvinsi.Text, text));
            }
        }

        private void OnProvinsiKeyDown(Object sender, KeyEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                if (e.KeyCode == Keys.Enter && this.iProvinsi.SelectedIndex != -1)
                {
                    _ = this.iKota.Focus();
                    this.iKota.ShowPopup();
                }
            }
        }

        private void OnKotaKeyDown(Object sender, KeyEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                if (e.KeyCode == Keys.Enter && this.iKota.SelectedIndex != -1)
                {
                    _ = this.iKecamatan.Focus();
                    this.iKecamatan.ShowPopup();
                }
            }
        }

        private void OnKecamatanKeyDown(Object sender, KeyEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                if (e.KeyCode == Keys.Enter && this.iKecamatan.SelectedIndex != -1)
                {
                    _ = this.iDesa.Focus();
                    this.iDesa.ShowPopup();
                }
            }
        }

        private void OnDesaKeyDown(Object sender, KeyEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                if (e.KeyCode == Keys.Enter && this.iDesa.SelectedIndex != -1)
                {
                    _ = this.iKodePos.Focus();
                }
            }
        }

        private void OnButtonResetInputClick(Object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    this.iProvinsi.SelectedIndex = -1;
                    this.iKota.SelectedIndex = -1;
                    this.iKecamatan.SelectedIndex = -1;
                    this.iDesa.SelectedIndex = -1;
                    this.iKodePos.Text = "";
                    this.iProvinsi.Properties.Items.Clear();
                    this.iKota.Properties.Items.Clear();
                    this.iKecamatan.Properties.Items.Clear();
                    this.iDesa.Properties.Items.Clear();
                    _ = this.iProvinsi.Focus();
                }
                finally
                {
                    this.m_SuppressEvents = false;
                }
            }
        }

        private void OnProvinsiSelectedIndexChanged(Object sender, EventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    var content = this.iProvinsi.SelectedItem as AlamatContent;
                    if (content is not null)
                    {
                        if (this.iKota.SelectedIndex == -1 || this.iKota.SelectedItem is not AlamatContent kotaInfo || kotaInfo.KotaID != content.KotaID)
                        {
                            this.iKota.SelectedIndex = -1;
                            this.iKota.Properties.Items.Clear();
                        }
                        if (this.iKecamatan.SelectedIndex == -1 || this.iKecamatan.SelectedItem is not AlamatContent kecamatanInfo || kecamatanInfo.KotaID != content.KotaID)
                        {
                            this.iKecamatan.SelectedIndex = -1;
                            this.iKecamatan.Properties.Items.Clear();
                        }
                        if (this.iDesa.SelectedIndex == -1 || this.iDesa.SelectedItem is not AlamatContent desaInfo || desaInfo.KotaID != content.KotaID)
                        {
                            this.iDesa.SelectedIndex = -1;
                            this.iDesa.Properties.Items.Clear();
                            this.iKodePos.Text = "";
                        }
                    }
                }
                finally
                {
                    this.SplashScreenClose();
                    this.m_SuppressEvents = false;
                }
            }
        }

        private async void OnKotaSelectedIndexChanged(Object sender, EventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    var content = this.iKota.SelectedItem as AlamatContent;
                    if (content is not null)
                    {
                        if (ShouldSynced(this.iKecamatan, AlamatMethod.ListKecamatan, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iKecamatan, AlamatMethod.ListKecamatan, content);
                        }
                        if (ShouldSynced(this.iProvinsi, AlamatMethod.ListProvinsi, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iProvinsi, AlamatMethod.ListProvinsi, content);
                        }
                        if (this.iKecamatan.SelectedIndex == -1 || this.iKecamatan.SelectedItem is not AlamatContent kecamatanInfo || kecamatanInfo.KotaID != content.KotaID)
                        {
                            this.iKecamatan.SelectedIndex = -1;
                            this.iKecamatan.Properties.Items.Clear();
                        }
                        if (this.iDesa.SelectedIndex == -1 || this.iDesa.SelectedItem is not AlamatContent desaInfo || desaInfo.KotaID != content.KotaID)
                        {
                            this.iDesa.SelectedIndex = -1;
                            this.iDesa.Properties.Items.Clear();
                            this.iKodePos.Text = "";
                        }
                    }
                }
                finally
                {
                    this.SplashScreenClose();
                    this.m_SuppressEvents = false;
                }
            }
        }

        private async void OnKecamatanSelectedIndexChanged(Object sender, EventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    var content = this.iKecamatan.SelectedItem as AlamatContent;
                    if (content is not null)
                    {
                        if (ShouldSynced(this.iKota, AlamatMethod.ListKota, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iKota, AlamatMethod.ListKota, content);
                        }
                        if (ShouldSynced(this.iProvinsi, AlamatMethod.ListProvinsi, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iProvinsi, AlamatMethod.ListProvinsi, content);
                        }
                        if (this.iDesa.SelectedIndex == -1 || this.iDesa.SelectedItem is not AlamatContent desaInfo || desaInfo.KecamatanID != content.KecamatanID)
                        {
                            this.iDesa.SelectedIndex = -1;
                            this.iDesa.Properties.Items.Clear();
                            this.iKodePos.Text = "";
                        }
                    }
                }
                finally
                {
                    this.SplashScreenClose();
                    this.m_SuppressEvents = false;
                }
            }
        }

        private async void OnDesaSelectedIndexChanged(Object sender, EventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    var content = this.iDesa.SelectedItem as AlamatContent;
                    if (content is not null)
                    {
                        if (ShouldSynced(this.iKecamatan, AlamatMethod.ListKecamatan, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iKecamatan, AlamatMethod.ListKecamatan, content);
                        }
                        if (ShouldSynced(this.iKota, AlamatMethod.ListKota, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iKota, AlamatMethod.ListKota, content);
                        }
                        if (ShouldSynced(this.iProvinsi, AlamatMethod.ListProvinsi, content))
                        {
                            this.SplashScreenShow();
                            await SynchronizeItems(this.iProvinsi, AlamatMethod.ListProvinsi, content);
                        }
                        this.iKodePos.Text = content.KodePos;
                    }
                    else
                    {
                        this.iKodePos.Text = "";
                    }
                }
                finally
                {
                    this.SplashScreenClose();
                    this.m_SuppressEvents = false;
                }
            }
        }

        private static Boolean ShouldSynced(ComboBoxEdit editor, AlamatMethod method, AlamatContent content)
        {
            if (editor.SelectedIndex == -1)
            {
                return true;
            }
            if (editor.SelectedItem is AlamatContent data)
            {
                return method switch
                {
                    AlamatMethod.ListProvinsi => data.ProvinsiID != content.ProvinsiID,
                    AlamatMethod.ListKota => data.KotaID != content.KotaID,
                    AlamatMethod.ListKecamatan => data.KecamatanID != content.KecamatanID,
                    AlamatMethod.ListDesa => data.DesaID != content.DesaID,
                    _ => true
                };
            }
            return true;
        }

        private async void OnProvinsiQueryPopup(Object sender, CancelEventArgs e)
            => await this.PopulateItems(this.iProvinsi, true, AlamatMethod.ListProvinsi, e);

        private async void OnKotaQueryPopup(Object sender, CancelEventArgs e)
            => await this.PopulateItems(this.iKota, true, AlamatMethod.ListKota, e);

        private async void OnKecamatanQueryPopup(Object sender, CancelEventArgs e)
            => await this.PopulateItems(this.iKecamatan, true, AlamatMethod.ListKecamatan, e);

        private async void OnDesaQueryPopup(Object sender, CancelEventArgs e)
            => await this.PopulateItems(this.iDesa, true, AlamatMethod.ListDesa, e);

        private async Task PopulateItems(ComboBoxEdit editor, Boolean onlyEmpty, AlamatMethod method, CancelEventArgs e)
        {
            if (e is not null) e.Cancel = false;
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    var items = editor.Properties.Items;
                    if (!onlyEmpty || items.Count == 0)
                    {
                        this.SplashScreenShow();
                        if (!onlyEmpty) items.Clear();
                        try
                        {
                            var list = method switch
                            {
                                AlamatMethod.ListProvinsi => await LoadProvinsi(),
                                AlamatMethod.ListKota => await LoadKota
                                (
                                    this.iProvinsi.SelectedIndex == -1 ? null : this.iProvinsi.SelectedItem as AlamatContent ?? (Object)this.iProvinsi.Text
                                ),
                                AlamatMethod.ListKecamatan => await LoadKecamatan
                                (
                                    this.iProvinsi.SelectedIndex == -1 ? null : this.iProvinsi.SelectedItem as AlamatContent ?? (Object)this.iProvinsi.Text,
                                    this.iKota.SelectedIndex == -1 ? null : this.iKota.SelectedItem as AlamatContent ?? (Object)this.iKota.Text
                                ),
                                AlamatMethod.ListDesa => await LoadDesa
                                (
                                    this.iProvinsi.SelectedIndex == -1 ? null : this.iProvinsi.SelectedItem as AlamatContent ?? (Object)this.iProvinsi.Text,
                                    this.iKota.SelectedIndex == -1 ? null : this.iKota.SelectedItem as AlamatContent ?? (Object)this.iKota.Text,
                                    this.iKecamatan.SelectedIndex == -1 ? null : this.iKecamatan.SelectedItem as AlamatContent ?? (Object)this.iKecamatan.Text
                                ),
                                _ => Array.Empty<AlamatContent>()
                            };
                            if (list.Length != 0)
                            {
                                for (var i = 0; i < list.Length; i++)
                                {
                                    var node = list[i];
                                    node.Method = method;
                                    items.Add(node);
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            this.DisplayError(error);
                        }
                        finally
                        {
                            this.SplashScreenClose();
                        }
                    }
                }
                finally
                {
                    this.m_SuppressEvents = false;
                }
            }
        }

        private static async Task SynchronizeItems(ComboBoxEdit editor, AlamatMethod method, AlamatContent content)
        {
            var items = editor.Properties.Items;
            if (items.Count == 0)
            {
                editor.Parent.Enabled = false;
                try
                {
                    var list = method switch
                    {
                        AlamatMethod.ListProvinsi => await LoadProvinsi(),
                        AlamatMethod.ListKota => await LoadKota(content.Provinsi),
                        AlamatMethod.ListKecamatan => await LoadKecamatan(content.Provinsi, content.Kota),
                        AlamatMethod.ListDesa => await LoadDesa(content.Provinsi, content.Kota, content.Kecamatan),
                        _ => Array.Empty<AlamatContent>()
                    };
                    for (var i = 0; i < list.Length; i++)
                    {
                        var node = list[i];
                        node.Method = method;
                        items.Add(node);
                    }
                }
                finally
                {
                    editor.Parent.Enabled = true;
                    editor.Parent.Refresh();
                }
            }
            var comparer = StringComparer.OrdinalIgnoreCase;
            var count = items.Count;
            var id = method switch
            {
                AlamatMethod.ListProvinsi => content.ProvinsiID,
                AlamatMethod.ListKota => content.KotaID,
                AlamatMethod.ListKecamatan => content.KecamatanID,
                AlamatMethod.ListDesa => content.DesaID,
                _ => 0
            };
            var hasID = id > 0;
            while (--count > -1)
            {
                var node = (AlamatContent)items[count];
                switch (method)
                {
                    case AlamatMethod.ListProvinsi:
                        if ((hasID && node.ProvinsiID > 0 && id == node.ProvinsiID) || comparer.Equals(node.Provinsi, content.Provinsi))
                        {
                            editor.SelectedIndex = count;
                            return;
                        }
                        break;
                    case AlamatMethod.ListKota:
                        if ((hasID && node.KotaID > 0 && id == node.KotaID) || comparer.Equals(node.Kota, content.Kota))
                        {
                            editor.SelectedIndex = count;
                            return;
                        }
                        break;
                    case AlamatMethod.ListKecamatan:
                        if ((hasID && node.KecamatanID > 0 && id == node.KecamatanID) || comparer.Equals(node.Kecamatan, content.Kecamatan))
                        {
                            editor.SelectedIndex = count;
                            return;
                        }
                        break;
                    case AlamatMethod.ListDesa:
                        if ((hasID && node.DesaID > 0 && id == node.DesaID) || comparer.Equals(node.Desa, content.Desa))
                        {
                            editor.SelectedIndex = count;
                            return;
                        }
                        break;
                }
            }
        }

        #endregion

        #region >>> Class Fields: Private fields yang di deklarasikan di dalam "instance" kelas ini

        private IOverlaySplashScreenHandle m_SplashHandle;
        private OverlayWindowOptions m_SplashOptions;
        private OverlayTextPainter m_SplashPainter;
        private Boolean m_SuppressEvents;

        #endregion


    }
}