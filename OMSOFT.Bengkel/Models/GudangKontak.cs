namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.GudangKontak")]
    public partial class GudangKontak
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDGudang { get; set; }

        [Required]
        [StringLength(300)]
        public string Nama { get; set; }

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

        [StringLength(50)]
        public string NoTelp1 { get; set; }

        [StringLength(50)]
        public string NoTelp2 { get; set; }

        [StringLength(50)]
        public string NoFaks1 { get; set; }

        [StringLength(50)]
        public string NoFaks2 { get; set; }

        [StringLength(50)]
        public string NoHp1 { get; set; }

        [StringLength(50)]
        public string NoHp2 { get; set; }

        [StringLength(50)]
        public string NoWa1 { get; set; }

        [StringLength(50)]
        public string NoWa2 { get; set; }

        [StringLength(200)]
        public string Email1 { get; set; }

        [StringLength(200)]
        public string Email2 { get; set; }

        [StringLength(200)]
        public string Website { get; set; }

        [StringLength(200)]
        public string Facebook { get; set; }

        [StringLength(200)]
        public string Instagram { get; set; }

        [StringLength(200)]
        public string Youtube { get; set; }

        [StringLength(200)]
        public string Twitter { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TglLahir { get; set; }

        public Guid? IDAgama { get; set; }

        public Guid? IDPhoto { get; set; }

        [StringLength(100)]
        public string Jabatan { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Agama Agama { get; set; }

        public virtual Gudang Gudang { get; set; }

        public virtual Photo Photo { get; set; }
    }
}
