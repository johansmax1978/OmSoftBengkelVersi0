namespace OMSOFT.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("aa.PerusahaanConfig"), JsonObject(MemberSerialization.OptIn)]
    public partial class PerusahaanConfig
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public Guid ID { get; set; }

        [Required]
        [StringLength(10)]
        [JsonProperty("default_locale_name", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string DefaultLocaleName { get; set; }

        [Required]
        [StringLength(50)]
        [JsonProperty("default_date_format", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string DefaultDateFormat { get; set; }

        [Required]
        [StringLength(50)]
        [JsonProperty("default_time_format", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string DefaultTimeFormat { get; set; }

        [Required]
        [StringLength(50)]
        [JsonProperty("full_date_time_format", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string FullDateTimeFormat { get; set; }

        [JsonProperty("use_number_grouping", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public bool UseNumberGrouping { get; set; }

        [JsonProperty("digits_decimal_default", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public byte DigitsDecimalDefault { get; set; }

        [JsonProperty("digits_decimal_report", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public byte? DigitsDecimalReport { get; set; }

        [JsonProperty("digits_decimal_gui", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public byte? DigitsDecimalGUI { get; set; }

        [StringLength(5)]
        [JsonProperty("separator_grouping", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string SeparatorGrouping { get; set; }

        [StringLength(5)]
        [JsonProperty("separator_decimals", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string SeparatorDecimals { get; set; }

        [StringLength(50)]
        [JsonProperty("default_app_theme", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string DefaultAppTheme { get; set; }

        [StringLength(50)]
        [JsonProperty("default_app_palette", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string DefaultAppPalette { get; set; }

        [JsonProperty("additional_settings", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public string AdditionalSettings { get; set; }

        [JsonProperty("last_edited", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include, Required = Required.Default)]
        public DateTimeOffset? LastEdited { get; set; }

        [JsonIgnore]
        public virtual Perusahaan Perusahaan { get; set; }
    }
}
