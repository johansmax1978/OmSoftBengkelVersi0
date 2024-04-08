namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.UserAccess")]
    public partial class UserAccess
    {
        public Guid ID { get; set; }

        public Guid IDGroup { get; set; }

        [Required]
        [StringLength(300)]
        public string Target { get; set; }

        public bool Create { get; set; }

        public bool Select { get; set; }

        public bool Insert { get; set; }

        public bool Update { get; set; }

        public bool Delete { get; set; }

        public bool Execute { get; set; }

        public bool Print { get; set; }

        [StringLength(500)]
        public string Keterangan { get; set; }

        public virtual UserGroup UserGroup { get; set; }
    }
}
