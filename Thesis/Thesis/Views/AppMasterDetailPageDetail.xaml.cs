using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppMasterDetailPageDetail : ContentPage
    {
        public static string datetime { get; set; }
        public static ConnectType connecttype { get; set; }

        public AppMasterDetailPageDetail()
        {
            InitializeComponent();
            servername.Text = connecttype.ConnectionName;
            serveruri.Text = "Uri:" + connecttype.ConnectionUrl;
            connectionstatus.Text = "Connected";
            connectedsince.Text = datetime;
        }
    }
}