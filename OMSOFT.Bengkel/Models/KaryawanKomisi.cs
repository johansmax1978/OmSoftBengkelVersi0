namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.KaryawanKomisi")]
    public partial class KaryawanKomisi
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDKaryawan { get; set; }

        [Required]
        [StringLength(50)]
        public string TipeItem { get; set; }

        [Required]
        [StringLength(50)]
        public string PengaturanKomisi { get; set; }

        public double? ProsentaseKomisi { get; set; }

        [StringLength(50)]
        public string KomisiDihitungDari { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Karyawan Karyawan { get; set; }
    }
}
