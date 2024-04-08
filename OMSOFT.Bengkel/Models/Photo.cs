namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("za.Photo")]
    public partial class Photo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Photo()
        {
            Cabang = new HashSet<Cabang>();
            CabangKontak = new HashSet<CabangKontak>();
            BarangPhoto = new HashSet<BarangPhoto>();
            Customer = new HashSet<Customer>();
            CustomerKontak = new HashSet<CustomerKontak>();
            Gudang = new HashSet<Gudang>();
            GudangKontak = new HashSet<GudangKontak>();
            Karyawan = new HashSet<Karyawan>();
            Supplier = new HashSet<Supplier>();
        }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid ID { get; set; }

        [Required]
        [StringLength(100)]
        [JsonProperty("mime", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Mime { get; set; }

        [Required]
        [StringLength(4)]
        [JsonProperty("mode", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Mode { get; set; }

        [StringLength(1024)]
        [JsonProperty("path", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Path { get; set; }

        [StringLength(2048)]
        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Include)]
        public string URL { get; set; }

        [JsonProperty("raw", DefaultValueHandling = DefaultValueHandling.Include)]
        public byte[] Raw { get; set; }

        [StringLength(100)]
        [JsonProperty("ref_name", DefaultValueHandling = DefaultValueHandling.Include)]
        public string RefName { get; set; }

        [StringLength(255)]
        [JsonProperty("ref_key", DefaultValueHandling = DefaultValueHandling.Include)]
        public string RefKey { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Cabang> Cabang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<CabangKontak> CabangKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<BarangPhoto> BarangPhoto { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Customer> Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<CustomerKontak> CustomerKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Gudang> Gudang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<GudangKontak> GudangKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Karyawan> Karyawan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Supplier> Supplier { get; set; }

        [JsonIgnore]
        public virtual SupplierKontak SupplierKontak { get; set; }
    }
}
