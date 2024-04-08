namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.Customer")]
    public partial class Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Customer()
        {
            CustomerAlamat = new HashSet<CustomerAlamat>();
            CustomerCabang = new HashSet<CustomerCabang>();
            CustomerKontak = new HashSet<CustomerKontak>();
            CustomerRekening = new HashSet<CustomerRekening>();
        }

        public bool StatusDelete { get; set; }

        public bool StatusAktif { get; set; }

        public Guid ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Kode { get; set; }

        [Required]
        [StringLength(300)]
        public string Nama { get; set; }

        public Guid? IDGroup { get; set; }

        public Guid? IDPhoto { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual CustomerGroup CustomerGroup { get; set; }

        public virtual Photo Photo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CustomerAlamat> CustomerAlamat { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<CustomerCabang> CustomerCabang { get; set; }

        public virtual CustomerCatatan CustomerCatatan { get; set; }

        public virtual CustomerConfig CustomerConfig { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CustomerKontak> CustomerKontak { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CustomerRekening> CustomerRekening { get; set; }
    }
}
