﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        private class AppMasterDetailPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<AppMasterDetailPageMenuItem> MenuItems { get; set; }

            public AppMasterDetailPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<AppMasterDetailPageMenuItem>(new[]
                {
                    new AppMasterDetailPageMenuItem { Id = 0,Icon="home.png", Title = "Status" ,TargetType=typeof(AppMasterDetailPageDetail)},
                    new AppMasterDetailPageMenuItem { Id = 1,Icon="connector.png", Title = "TreeView" ,TargetType=typeof(TreeView)},
                    new AppMasterDetailPageMenuItem { Id = 2,Icon="hardware.png", Title = "Monitor", TargetType=typeof(Monitor)},
                    new AppMasterDetailPageMenuItem { Id = 3,Icon="graph.png", Title = "LineGraph", TargetType=typeof(LineGraphPage)},
                    new AppMasterDetailPageMenuItem { Id = 4,Icon="doubt.png", Title = "Help" ,TargetType=typeof(HelpPage)},
                    new AppMasterDetailPageMenuItem { Id = 5,Icon="info.png", Title = "About" ,TargetType=typeof(AboutPage)},
                });
            }

            #region INotifyPropertyChanged Implementation

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion INotifyPropertyChanged Implementation
        }

        public static SampleClient OpcClient_Master { get; set; }

        private void Disconnect_Clicked(object sender, EventArgs e)
        {
            OpcClient_Master.Disconnect(OpcClient_Master.session);
            App.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}