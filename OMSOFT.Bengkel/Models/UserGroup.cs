namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.UserGroup")]
    public partial class UserGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserGroup()
        {
            User = new HashSet<User>();
            UserAccess = new HashSet<UserAccess>();
        }

        public bool StatusDelete { get; set; }

        public bool StatusAktif { get; set; }

        public Guid ID { get; set; }

        [Required]
        [StringLength(300)]
        public string Nama { get; set; }

        [StringLength(1000)]
        public string Keterangan { get; set; }

        public bool? MultiCabang { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> User { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAccess> UserAccess { get; set; }
    }
}
