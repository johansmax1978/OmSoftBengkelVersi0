namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.PerusahaanAlamat"), JsonObject(MemberSerialization.OptIn)]
    public partial class PerusahaanAlamat
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public Guid ID { get; set; }

        [StringLength(2000)]
        [JsonProperty("alamat", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Alamat { get; set; }

        [StringLength(100)]
        [JsonProperty("desa", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Desa { get; set; }

        [StringLength(100)]
        [JsonProperty("kecamatan", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Kecamatan { get; set; }

        [StringLength(100)]
        [JsonProperty("kota", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Kota { get; set; }

        [StringLength(100)]
        [JsonProperty("provinsi", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Provinsi { get; set; }

        [StringLength(50)]
        [JsonProperty("notelp1", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoTelp1 { get; set; }

        [StringLength(50)]
        [JsonProperty("notelp2", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoTelp2 { get; set; }

        [StringLength(50)]
        [JsonProperty("nofaks1", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoFaks1 { get; set; }

        [StringLength(50)]
        [JsonProperty("nofaks2", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoFaks2 { get; set; }

        [StringLength(50)]
        [JsonProperty("nohp1", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoHp1 { get; set; }

        [StringLength(50)]
        [JsonProperty("nohp2", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoHp2 { get; set; }

        [StringLength(50)]
        [JsonProperty("nowa1", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoWa1 { get; set; }

        [StringLength(50)]
        [JsonProperty("nowa2", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoWa2 { get; set; }

        [StringLength(200)]
        [JsonProperty("email1", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Email1 { get; set; }

        [StringLength(200)]
        [JsonProperty("email2", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Email2 { get; set; }

        [StringLength(200)]
        [JsonProperty("website", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Website { get; set; }

        [StringLength(200)]
        [JsonProperty("facebook", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Facebook { get; set; }

        [StringLength(200)]
        [JsonProperty("instagram", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Instagram { get; set; }

        [StringLength(200)]
        [JsonProperty("youtube", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Youtube { get; set; }

        [StringLength(200)]
        [JsonProperty("twitter", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Twitter { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual Perusahaan Perusahaan { get; set; }
    }
}
