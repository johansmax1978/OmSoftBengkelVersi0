namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.KaryawanCatatan")]
    public partial class KaryawanCatatan
    {
        public Guid ID { get; set; }

        [Required]
        public string Catatan { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Karyawan Karyawan { get; set; }
    }
}
