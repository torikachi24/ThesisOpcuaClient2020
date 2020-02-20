using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private StackLayout stacklayout = new StackLayout();
        private static LabelViewModel textInfo = new LabelViewModel();
        private SampleClient OpcClient = new SampleClient(textInfo);
        private string endpointUrl = null;

        public MainPage()
        {
            BindingContext = textInfo;
            InitializeComponent();
        }

        private async void OnConnect(object sender, EventArgs e)
        {
            endpointUrl = EntryUrl.Text;

            if (endpointUrl != null)
            {
                if (ConnectButton.Text == "Connect")
                {
                    bool connectToServer = true;
                    ConnectIndicator.IsRunning = true;

                    await Task.Run(() => OpcClient.CreateCertificate());

                    if (OpcClient.haveAppCertificate == false)
                    {
                        connectToServer = await DisplayAlert("Warning", "missing application certificate, \nusing unsecure connection. \nDo you want to continue?", "Yes", "No");
                    }

                    if (connectToServer == true)
                    {
                        var connectionStatus = await Task.Run(() => OpcClient.OpcClient(endpointUrl));

                        if (connectionStatus == SampleClient.ConnectionStatus.Connected)
                        {
                            Tree tree;
                            ConnectButton.Text = "Disconnect";

                            tree = OpcClient.GetRootNode(textInfo);
                            if (tree.currentView[0].children == true)
                            {
                                tree = OpcClient.GetChildren(tree.currentView[0].id);
                            }

                            ConnectIndicator.IsRunning = false;
                            //if (Device.OS == TargetPlatform.Android)
                            //{
                            //    //Application.Current.MainPage = new MasterDetail();
                            //}
                            //else if (Device.OS == TargetPlatform.iOS)
                            //{
                            //    await Navigation.PushModalAsync(new MasterDetail());
                            //}
                            Page treeViewRoot = new TreeView(tree, OpcClient);
                            treeViewRoot.Title = "/Root";
                            await Navigation.PushAsync(treeViewRoot);
                        }
                        else
                        {
                            ConnectIndicator.IsRunning = false;
                            await DisplayAlert("Warning", "Cannot connect to an OPC UA server", "Ok");
                        }
                    }
                    else
                    {
                        ConnectIndicator.IsRunning = false;
                    }
                }
                else
                {
                    OpcClient.Disconnect(OpcClient.session);
                    ConnectButton.Text = "Connect";
                }
            }
            else
            {
                await DisplayAlert("Warning", "Server endpoint URL cannot be null", "Ok");
            }
        }

        private async void AddNewConnection_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new Popup());
        }
    }
}