namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.KaryawanKtp")]
    public partial class KaryawanKtp
    {
        public Guid ID { get; set; }

        [StringLength(50)]
        public string NoKtp { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TglLahir { get; set; }

        [StringLength(50)]
        public string JenisKelamin { get; set; }

        [StringLength(10)]
        public string GolDarah { get; set; }

        [StringLength(2000)]
        public string Alamat { get; set; }

        [StringLength(100)]
        public string Desa { get; set; }

        [StringLength(100)]
        public string Kecamatan { get; set; }

        [StringLength(100)]
        public string Kota { get; set; }

        [StringLength(100)]
        public string Provinsi { get; set; }

        public Guid? IDAgama { get; set; }

        [StringLength(50)]
        public string StatusKawin { get; set; }

        [StringLength(50)]
        public string KewargaNegaraan { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Agama Agama { get; set; }

        public virtual Karyawan Karyawan { get; set; }
    }
}
