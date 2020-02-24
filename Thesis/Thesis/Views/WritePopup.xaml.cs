using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WritePopup
    {
        public WritePopup()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}