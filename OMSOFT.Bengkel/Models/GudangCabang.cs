namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.GudangCabang")]
    public partial class GudangCabang
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDGudang { get; set; }

        public Guid IDCabang { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Cabang Cabang { get; set; }

        public virtual Gudang Gudang { get; set; }
    }
}
