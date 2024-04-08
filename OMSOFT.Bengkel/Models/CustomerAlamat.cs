namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.CustomerAlamat")]
    public partial class CustomerAlamat
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDCustomer { get; set; }

        [Required]
        [StringLength(50)]
        public string TipeAlamat { get; set; }

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

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
