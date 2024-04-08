namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.Cabang")]
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Cabang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cabang()
        {
            BarangCabang = new HashSet<BarangCabang>();
            CabangAlamat = new HashSet<CabangAlamat>();
            CabangKontak = new HashSet<CabangKontak>();
            CustomerCabang = new HashSet<CustomerCabang>();
            GudangCabang = new HashSet<GudangCabang>();
            KaryawanCabang = new HashSet<KaryawanCabang>();
            SupplierCabang = new HashSet<SupplierCabang>();
        }

        [JsonProperty("status_delete", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StatusDelete { get; set; }

        [JsonProperty("status_aktif", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StatusAktif { get; set; }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid ID { get; set; }

        [JsonProperty("id_perusahaan", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid IDPerusahaan { get; set; }

        [JsonProperty("no_urut", DefaultValueHandling = DefaultValueHandling.Include)]
        public int NoUrut { get; set; }

        [Required]
        [StringLength(100)]
        [JsonProperty("kode", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Kode { get; set; }

        [Required]
        [StringLength(300)]
        [JsonProperty("nama", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Nama { get; set; }

        [JsonProperty("id_photo", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid? IDPhoto { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include)]
        public DateTimeOffset? LastEdited { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<BarangCabang> BarangCabang { get; set; }

        [JsonIgnore]
        public virtual Perusahaan Perusahaan { get; set; }

        [JsonProperty("ref_photo", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual Photo Photo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonProperty("ref_alamat", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual ICollection<CabangAlamat> CabangAlamat { get; set; }

        [JsonProperty("ref_catatan", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual CabangCatatan CabangCatatan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonProperty("ref_kontak", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual ICollection<CabangKontak> CabangKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<CustomerCabang> CustomerCabang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<GudangCabang> GudangCabang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<KaryawanCabang> KaryawanCabang { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<SupplierCabang> SupplierCabang { get; set; }
    }
}
