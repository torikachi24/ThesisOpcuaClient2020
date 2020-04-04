﻿using Rg.Plugins.Popup.Services;
using System;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonitorPage : TabbedPage
    {
        public MonitorPage()
        {
            InitializeComponent();
            //BindingContext = new MonitorListViewModel();
        }

        private void TapGestureRecognizer_Tapped_Remove(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            MonitorType monitorType = ((MonitorListViewModel)BindingContext).Monitors.Where(emp => emp.MonitorId == (int)tappedEventArgs.Parameter).FirstOrDefault();
            ((MonitorListViewModel)BindingContext).Monitors.Remove(monitorType);
        }

        private async void ToolbarItem_Clicked_About(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutPage());
        }

        private async void ToolbarItem_Clicked_Help(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new HelpPage());
        }
    }
}