using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppMasterDetailPageMaster : ContentPage
    {
        public ListView ListView;

        public AppMasterDetailPageMaster()
        {
            InitializeComponent();

            BindingContext = new AppMasterDetailPageMasterViewModel();
            ListView = MenuItemsListView;
        }

        class AppMasterDetailPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<AppMasterDetailPageMenuItem> MenuItems { get; set; }
            
            public AppMasterDetailPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<AppMasterDetailPageMenuItem>(new[]
                {
                    new AppMasterDetailPageMenuItem { Id = 0,Icon="home.png", Title = "Status" ,TargetType=typeof(MainPage)},
                    new AppMasterDetailPageMenuItem { Id = 1,Icon="search.png", Title = "TreeView" ,TargetType=typeof(TreeView)},
                    new AppMasterDetailPageMenuItem { Id = 2,Icon="report.png", Title = "Monitor", TargetType=typeof(Monitor)},
                    new AppMasterDetailPageMenuItem { Id = 3,Icon="question.png", Title = "Help" ,TargetType=typeof(HelpPage)},
                    new AppMasterDetailPageMenuItem { Id = 4,Icon="info.png", Title = "About" ,TargetType=typeof(AboutPage)},
                });
            }
            
            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
        private static LabelViewModel textInfo = new LabelViewModel();
        private SampleClient OpcClient = new SampleClient(textInfo);
        private void Disconnect_Clicked(object sender, EventArgs e)
        {
            OpcClient.Disconnect(OpcClient.session);
            //DisplayAlert("Show", "Test", "OK");
        }
    }
}