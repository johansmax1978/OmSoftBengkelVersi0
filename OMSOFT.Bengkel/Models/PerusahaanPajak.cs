namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.PerusahaanPajak"), JsonObject(MemberSerialization.OptIn)]
    public partial class PerusahaanPajak
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public Guid ID { get; set; }

        [StringLength(300)]
        [JsonProperty("nama", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Nama { get; set; }

        [Column(TypeName = "date")]
        [JsonProperty("tgl_pkp", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public DateTime? TglPKP { get; set; }

        [StringLength(50)]
        [JsonProperty("no_pkp", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NoPKP { get; set; }

        [StringLength(50)]
        [JsonProperty("tipe_usaha", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string TipeUsaha { get; set; }

        [StringLength(50)]
        [JsonProperty("npwp", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string NPWP { get; set; }

        [StringLength(50)]
        [JsonProperty("klu", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string KLU { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual Perusahaan Perusahaan { get; set; }
    }
}
