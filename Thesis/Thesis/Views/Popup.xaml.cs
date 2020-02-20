using Rg.Plugins.Popup.Services;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Popup
    {
        public Popup(ConnectType connectType = null)
        {
            InitializeComponent();
            BindingContext = new AddOrEditConnectionViewModel();
            if (connectType != null)
            {
                ((AddOrEditConnectionViewModel)BindingContext).ConnectType = connectType;
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void AddConnection_Clicked(object sender, EventArgs e)
        {
            ConnectType connectType = ((AddOrEditConnectionViewModel)BindingContext).ConnectType;
            MessagingCenter.Send(this, "AddOrEditConnection", connectType);
            PopupNavigation.Instance.PopAsync();
        }
    }
}