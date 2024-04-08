namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.CabangCatatan")]
    public partial class CabangCatatan
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid ID { get; set; }

        [Required]
        [JsonProperty("catatan", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Catatan { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include)]
        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual Cabang Cabang { get; set; }
    }
}
