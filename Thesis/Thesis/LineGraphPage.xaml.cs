using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using SkiaSharp;
using Entry = Microcharts.Entry;
using Microcharts;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LineGraphPage : ContentPage
    {
        //List <Entry> entries = new List<Entry>
        //{
        //    new Entry(200)
        //    {
        //        Color=SKColor.Parse("#FF1493"),
        //        Label="January",
        //        ValueLabel="200"
        //    },
        //    new Entry(400)
        //    {
        //        Color=SKColor.Parse("#00BFFF"),
        //        Label="Febuary",
        //        ValueLabel="400"
        //    },
        //    new Entry(-100)
        //    {
        //        Color=SKColor.Parse("#00CED1"),
        //        Label="March",
        //        ValueLabel="-100"
        //    }
        //};
    
    public LineGraphPage()
        {
            InitializeComponent();
            //Chart1.Chart = new BarChart { Entries = entries };
            //Chart1.Chart = new LineChart { Entries = entries };
            //Chart1.Chart = new PointChart { Entries = entries };

            //Chart1.Chart = new RadialGaugeChart { Entries = entries };
            //Chart1.Chart = new DonutChart { Entries = entries };
            //Chart1.Chart = new RadarChart { Entries = entries };
    }
    }
}