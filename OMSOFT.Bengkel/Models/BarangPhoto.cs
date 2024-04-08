namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.BarangPhoto")]
    public partial class BarangPhoto
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDBarang { get; set; }

        public Guid IDPhoto { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Barang Barang { get; set; }

        public virtual Photo Photo { get; set; }
    }
}
