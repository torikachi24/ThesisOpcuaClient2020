using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Thesis
{
    public class Connection
    {
        public ICommand Clickabc => new Command(WriteConnection);
        public string Name { get; set; }
        public Connection()
        {
            Name = "aaa";
        }
        public async void WriteConnection()
        {
            Console.WriteLine("aaa");
            await App.Current.MainPage.DisplayAlert("Alert", "your message", "OK");
        }
    }
}
