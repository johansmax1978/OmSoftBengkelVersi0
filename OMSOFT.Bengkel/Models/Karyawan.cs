namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.Karyawan")]
    public partial class Karyawan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Karyawan()
        {
            KaryawanCabang = new HashSet<KaryawanCabang>();
            KaryawanKomisi = new HashSet<KaryawanKomisi>();
            KaryawanRekening = new HashSet<KaryawanRekening>();
        }

        public bool StatusDelete { get; set; }

        public bool StatusAktif { get; set; }

        public Guid ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Kode { get; set; }

        [Required]
        [StringLength(300)]
        public string Nama { get; set; }

        public Guid? IDJabatan { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TglMasuk { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TglKeluar { get; set; }

        [StringLength(50)]
        public string StatusPekerja { get; set; }

        public bool StatusKomisi { get; set; }

        public Guid? IDPhoto { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual User User { get; set; }

        public virtual Jabatan Jabatan { get; set; }

        public virtual Photo Photo { get; set; }

        public virtual KaryawanAlamat KaryawanAlamat { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KaryawanCabang> KaryawanCabang { get; set; }

        public virtual KaryawanCatatan KaryawanCatatan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KaryawanKomisi> KaryawanKomisi { get; set; }

        public virtual KaryawanKtp KaryawanKtp { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KaryawanRekening> KaryawanRekening { get; set; }
    }
}
