namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.Gudang")]
    public partial class Gudang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Gudang()
        {
            GudangCabang = new HashSet<GudangCabang>();
            GudangKontak = new HashSet<GudangKontak>();
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

        public Guid? IDPhoto { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Photo Photo { get; set; }

        public virtual GudangAlamat GudangAlamat { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GudangCabang> GudangCabang { get; set; }

        public virtual GudangCatatan GudangCatatan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GudangKontak> GudangKontak { get; set; }
    }
}
