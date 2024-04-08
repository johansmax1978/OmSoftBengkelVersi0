namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.User")]
    public partial class User
    {
        public bool StatusDelete { get; set; }

        public bool StatusAktif { get; set; }

        public Guid ID { get; set; }

        public Guid IDGroup { get; set; }

        [StringLength(150)]
        public string LoginUser { get; set; }

        [StringLength(20)]
        public string LoginNoHP { get; set; }

        [StringLength(150)]
        public string LoginEmail { get; set; }

        [Required]
        [StringLength(300)]
        public string LoginHash { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Karyawan Karyawan { get; set; }

        public virtual UserGroup UserGroup { get; set; }
    }
}
