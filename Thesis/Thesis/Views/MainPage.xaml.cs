using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
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
            BindingContext = new ConnectionListViewModel();
        }

        private async void OnConnect(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            ConnectType connectType = ((ConnectionListViewModel)BindingContext).Connections.Where(emp => emp.ConnectionId == (int)tappedEventArgs.Parameter).FirstOrDefault();
            endpointUrl = connectType.ConnectionUrl;
            //endpointUrl = EntryUrl.Text;

            if (endpointUrl != null)
            {
                if (ConnectButton.Text == "Connect")
                {
                    bool connectToServer = true;
                    //ConnectIndicator.IsRunning = true;
                    await PopupNavigation.Instance.PushAsync(new ActivityIndicatorPage());

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
                            await PopupNavigation.Instance.PopAsync();//turn off Activity Indicator
                            
                            
                            ///////////////////////////////////////
                            ConnectButton.Text = "Disconnect";
                            Tree tree;
                            tree = OpcClient.GetRootNode(textInfo);
                            if (tree.currentView[0].children == true)
                            {
                                tree = OpcClient.GetChildren(tree.currentView[0].id);
                            }
                            //ConnectIndicator.IsRunning = false;
                            //Page treeViewRoot = new TreeView(tree, OpcClient);
                            //treeViewRoot.Title = "/Root";
                            //await Navigation.PushAsync(treeViewRoot);
                            AppMasterDetailPage master = new AppMasterDetailPage(tree,OpcClient);
                            NavigationPage.SetHasBackButton(master, false);
                            await Navigation.PushAsync(master);
                        }
                        else
                        {
                            //ConnectIndicator.IsRunning = false;
                            await PopupNavigation.Instance.PopAsync();
                            await DisplayAlert("Warning", "Cannot connect to an OPC UA server", "Ok");
                        }
                    }
                    else
                    {
                        await PopupNavigation.Instance.PopAsync();
                        //ConnectIndicator.IsRunning = false;
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

        private async void TapGestureRecognizer_Tapped_Edit(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            ConnectType connectType = ((ConnectionListViewModel)BindingContext).Connections.Where(emp => emp.ConnectionId == (int)tappedEventArgs.Parameter).FirstOrDefault();
            await PopupNavigation.Instance.PushAsync(new Popup(connectType));
        }

        private void TapGestureRecognizer_Tapped_Remove(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            ConnectType connectType = ((ConnectionListViewModel)BindingContext).Connections.Where(emp => emp.ConnectionId == (int)tappedEventArgs.Parameter).FirstOrDefault();

            ((ConnectionListViewModel)BindingContext).Connections.Remove(connectType);
        }
    }
}