namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.CustomerHarga")]
    public partial class CustomerHarga
    {
        public bool StatusDelete { get; set; }

        public Guid ID { get; set; }

        public Guid IDCustomerGroup { get; set; }

        [Required]
        [StringLength(50)]
        public string TipeItem { get; set; }

        [StringLength(50)]
        public string LevelHarga { get; set; }

        public double? DiskonHarga { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual CustomerGroup CustomerGroup { get; set; }
    }
}
