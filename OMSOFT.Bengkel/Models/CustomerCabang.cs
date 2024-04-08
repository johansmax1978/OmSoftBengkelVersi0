namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.CustomerCabang")]
    public partial class CustomerCabang
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDCustomer { get; set; }

        public Guid IDCabang { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Cabang Cabang { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
