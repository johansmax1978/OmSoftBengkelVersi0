namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.SupplierCabang")]
    public partial class SupplierCabang
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDSupplier { get; set; }

        public Guid IDCabang { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Cabang Cabang { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
