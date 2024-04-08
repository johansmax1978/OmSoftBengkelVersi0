namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.CabangAlamat")]
    public partial class CabangAlamat
    {
        [JsonProperty("status_delete", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StatusDelete { get; set; }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid ID { get; set; }

        [JsonProperty("id_cabang", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid IDCabang { get; set; }

        [Required]
        [StringLength(50)]
        [JsonProperty("tipe_alamat", DefaultValueHandling = DefaultValueHandling.Include)]
        public string TipeAlamat { get; set; }

        [StringLength(2000)]
        [JsonProperty("alamat", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Alamat { get; set; }

        [StringLength(100)]
        [JsonProperty("desa", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Desa { get; set; }

        [StringLength(100)]
        [JsonProperty("kecamatan", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Kecamatan { get; set; }

        [StringLength(100)]
        [JsonProperty("kota", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Kota { get; set; }

        [StringLength(100)]
        [JsonProperty("provinsi", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Provinsi { get; set; }

        [StringLength(50)]
        [JsonProperty("no_telp_1", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoTelp1 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_telp_2", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoTelp2 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_faks_1", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoFaks1 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_faks_2", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoFaks2 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_hp_1", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoHp1 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_hp_2", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoHp2 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_wa_1", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoWa1 { get; set; }

        [StringLength(50)]
        [JsonProperty("no_wa_2", DefaultValueHandling = DefaultValueHandling.Include)]
        public string NoWa2 { get; set; }

        [StringLength(200)]
        [JsonProperty("email_1", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Email1 { get; set; }

        [StringLength(200)]
        [JsonProperty("email_2", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Email2 { get; set; }

        [StringLength(200)]
        [JsonProperty("website", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Website { get; set; }

        [StringLength(200)]
        [JsonProperty("facebook", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Facebook { get; set; }

        [StringLength(200)]
        [JsonProperty("instagram", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Instagram { get; set; }

        [StringLength(200)]
        [JsonProperty("youtube", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Youtube { get; set; }

        [StringLength(200)]
        [JsonProperty("twitter", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Twitter { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include)]
        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual Cabang Cabang { get; set; }
    }
}
