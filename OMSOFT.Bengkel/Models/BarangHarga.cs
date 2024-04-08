namespace OMSOFT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("ab.BarangHarga")]
    public partial class BarangHarga
    {
        public bool StatusDelete { get; set; }

        public bool TipeSatuan { get; set; }

        public Guid ID { get; set; }

        public Guid IDBarang { get; set; }

        public Guid IDSatuan { get; set; }

        public double IsiSatuan { get; set; }

        public decimal? HargaJual1 { get; set; }

        public decimal? HargaJual2 { get; set; }

        public decimal? HargaJual3 { get; set; }

        public decimal? HargaJual4 { get; set; }

        public decimal? HargaJual5 { get; set; }

        public double? ProsentaseKomisi { get; set; }

        [StringLength(50)]
        public string KomisiDihitungDari { get; set; }

        [StringLength(100)]
        public string KodeBarcode { get; set; }

        public DateTimeOffset? LastEdited { get; set; }

        public virtual Barang Barang { get; set; }

        public virtual BarangSatuan BarangSatuan { get; set; }
    }
}
