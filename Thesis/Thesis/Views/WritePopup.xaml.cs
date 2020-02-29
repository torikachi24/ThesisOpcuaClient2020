using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WritePopup
    {
        private SampleClient opcClient;
        private ListNode selected;
        public WritePopup(SampleClient Client,ListNode id)
        {
            InitializeComponent();
            opcClient = Client;
            selected = id;
        }

        private void Button_Clicked_Cancel(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            
        }

        private void Button_Clicked_Change(object sender, EventArgs e)
        {
            object value = ValueChange.Text;
            //DisplayAlert("Title", value, "OK", "Cancel");
            string abc = "ns=3;s=\"Start\"";
            opcClient.VariableWrite(abc, value);
            //opcClient.VariableWrite(selected.id, value);
            PopupNavigation.Instance.PopAllAsync();


        }
    }
} 