namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("za.Logo")]
    public partial class Logo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Logo()
        {
            Perusahaan = new HashSet<Perusahaan>();
        }

        public Guid ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Mime { get; set; }

        [Required]
        [StringLength(4)]
        public string Mode { get; set; }

        [StringLength(1024)]
        public string Path { get; set; }

        [StringLength(2048)]
        public string URL { get; set; }

        public byte[] Raw { get; set; }

        [StringLength(100)]
        public string RefName { get; set; }

        [StringLength(255)]
        public string RefKey { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Perusahaan> Perusahaan { get; set; }
    }
}
