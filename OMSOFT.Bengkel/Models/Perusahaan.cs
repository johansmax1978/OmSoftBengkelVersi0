namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.Perusahaan"), JsonObject(MemberSerialization.OptIn)]
    public partial class Perusahaan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Perusahaan()
        {
            Cabang = new HashSet<Cabang>();
        }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Default)]
        public Guid ID { get; set; }

        [JsonProperty("id_logo", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public Guid? IDLogo { get; set; }

        [Required]
        [StringLength(300)]
        [JsonProperty("nama", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string Nama { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public DateTimeOffset? LastEdited { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cabang> Cabang { get; set; }

        public virtual Logo Logo { get; set; }

        [JsonProperty("alamat", NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public virtual PerusahaanAlamat PerusahaanAlamat { get; set; }

        [JsonProperty("config", NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public virtual PerusahaanConfig PerusahaanConfig { get; set; }

        [JsonProperty("pajak", NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public virtual PerusahaanPajak PerusahaanPajak { get; set; }
    }
}
