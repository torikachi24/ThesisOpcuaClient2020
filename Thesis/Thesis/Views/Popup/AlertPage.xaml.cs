﻿using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertPage
    {
        public AlertPage(string alarm)
        {
            InitializeComponent();
            warningtext.Text = alarm;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}