namespace OMSOFT.Bengkel.Views.Master
{
    using DevExpress.XtraEditors;
    using OMSOFT.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class PerusahaanMasterEditDialog : DevExpress.XtraEditors.XtraForm
    {
        public PerusahaanMasterEditDialog()
        {
            InitializeComponent();
        }

        public Perusahaan Data
        {
            get => this.perusahaan1.Data;
            set => this.perusahaan1.Data = value;
        }

        private void perusahaan1_Load(object sender, EventArgs e)
        {

        }
    }
}