namespace OMSOFT.Bengkel.Controllers
{
    using OMSOFT.Bengkel.Models;
    using OMSOFT.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public partial class PerusahaanController : BaseController
    {
        public PerusahaanController() { }

        protected override String RouteName => "perusahaan";

        public async Task<Perusahaan> Get()
            => await this.HttpGetModel<Perusahaan>(GenerateURI(this, null)).ConfigureAwait(false);

        public Task<Perusahaan> Post(Perusahaan perusahaan)
            => this.HttpPostModelData(GenerateURI(this, null), perusahaan);
    }
}
