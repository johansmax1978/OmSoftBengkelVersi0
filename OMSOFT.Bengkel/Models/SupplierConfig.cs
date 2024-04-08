namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.SupplierConfig")]
    public partial class SupplierConfig
    {
        public Guid ID { get; set; }

        public int? HariJatuhTempo { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
