namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.Barang")]
    public partial class Barang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Barang()
        {
            BarangCabang = new HashSet<BarangCabang>();
            BarangPhoto = new HashSet<BarangPhoto>();
            BarangHarga = new HashSet<BarangHarga>();
        }

        public bool StatusDelete { get; set; }

        public bool StatusAktif { get; set; }

        public Guid ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Jenis { get; set; }

        [Required]
        [StringLength(100)]
        public string Kode { get; set; }

        [Required]
        [StringLength(400)]
        public string Nama { get; set; }

        public Guid? IDGroup { get; set; }

        public Guid? IDSubGroup { get; set; }

        public Guid? IDRak { get; set; }

        public double Pajak { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual BarangGroup BarangGroup { get; set; }

        public virtual BarangRak BarangRak { get; set; }

        public virtual BarangSubGroup BarangSubGroup { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BarangCabang> BarangCabang { get; set; }

        public virtual BarangCatatan BarangCatatan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BarangPhoto> BarangPhoto { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BarangHarga> BarangHarga { get; set; }
    }
}
