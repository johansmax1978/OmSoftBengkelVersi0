namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.CustomerConfig")]
    public partial class CustomerConfig
    {
        public Guid ID { get; set; }

        public int? HariJatuhTempo { get; set; }

        public decimal? BatasPiutangPerNota { get; set; }

        public int? BatasHariPiutang { get; set; }

        public decimal? BatasTotalPiutang { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
