namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.KaryawanRekening")]
    public partial class KaryawanRekening
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDKaryawan { get; set; }

        [StringLength(50)]
        public string NoRekening { get; set; }

        [StringLength(300)]
        public string AnRekening { get; set; }

        [StringLength(300)]
        public string BankRekening { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Karyawan Karyawan { get; set; }
    }
}
