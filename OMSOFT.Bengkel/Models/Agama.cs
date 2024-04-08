namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    [Table("ab.Agama")]
    public partial class Agama
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Agama()
        {
            CabangKontak = new HashSet<CabangKontak>();
            CustomerKontak = new HashSet<CustomerKontak>();
            GudangKontak = new HashSet<GudangKontak>();
            KaryawanKtp = new HashSet<KaryawanKtp>();
            SupplierKontak = new HashSet<SupplierKontak>();
        }

        [JsonProperty("status_delete", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StatusDelete { get; set; }

        [JsonProperty("status_aktif", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StatusAktif { get; set; }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid ID { get; set; }

        [Required]
        [StringLength(300)]
        [JsonProperty("nama", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Nama { get; set; }

        [StringLength(1000)]
        [JsonProperty("keterangan", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Keterangan { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include)]
        public DateTimeOffset? LastEdited { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<CabangKontak> CabangKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<CustomerKontak> CustomerKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<GudangKontak> GudangKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<KaryawanKtp> KaryawanKtp { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<SupplierKontak> SupplierKontak { get; set; }
    }
}
