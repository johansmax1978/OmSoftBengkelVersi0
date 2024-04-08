namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.CabangKontak")]
    public partial class CabangKontak
    {
        [JsonProperty("status_delete", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StatusDelete { get; set; }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid ID { get; set; }

        [JsonProperty("id_cabang", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid IDCabang { get; set; }

        [Required]
        [StringLength(300)]
        [JsonProperty("nama", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Nama { get; set; }

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

        [Column(TypeName = "date")]
        [JsonProperty("tgl_lahir", DefaultValueHandling = DefaultValueHandling.Include)]
        public DateTime? TglLahir { get; set; }

        [JsonProperty("id_agama", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid? IDAgama { get; set; }

        [JsonProperty("id_photo", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid? IDPhoto { get; set; }

        [StringLength(100)]
        [JsonProperty("jabatan", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Jabatan { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include)]
        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual Cabang Cabang { get; set; }

        [JsonProperty("agama", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual Agama Agama { get; set; }

        [JsonProperty("photo", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual Photo Photo { get; set; }
    }
}
