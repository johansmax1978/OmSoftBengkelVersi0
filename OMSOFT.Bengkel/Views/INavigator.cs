namespace OMSOFT.Bengkel.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial interface INavigator
    {
        event EventHandler<NavigasiEvent> Navigate;

        String Caption { get; set; }

        Control Control { get; }
    }

    public class NavigasiEvent : EventArgs
    {
        public NavigasiEvent(String destination, Object parameters)
        {
            this.Destination = destination;
            this.Parameters = parameters;
        }

        public String Destination { get; }

        public Object Parameters { get; }
    }
}
