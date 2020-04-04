using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Thesis;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPageMonitor : TabbedPage
    {
        public TabbedPageMonitor()
        {
            BindingContext = new MonitorListViewModel();
            InitializeComponent();
            
        }

        private void TapGestureRecognizer_Tapped_Remove(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            MonitorType monitorType = ((MonitorListViewModel)BindingContext).Monitors.Where(emp => emp.MonitorId == (int)tappedEventArgs.Parameter).FirstOrDefault();

            ((MonitorListViewModel)BindingContext).Monitors.Remove(monitorType);
        }
    }
}