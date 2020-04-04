using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Test : ContentPage
	{
        private Int16 itemCount;
        private MonitoredItem myMonitoredItem;
        public static SampleClient opc;
        private Subscription mySubscription;
        private ObservableCollection<MonitorType> Monitors { get; set; }


        public Test()
		{
			InitializeComponent ();
            OnSubcription();
            Listu.ItemsSource = Monitors;
		}
        

        private void TapGestureRecognizer_Tapped_Remove(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            MonitorType monitorType = ((MonitorListViewModel)BindingContext).Monitors.Where(emp => emp.MonitorId == (int)tappedEventArgs.Parameter).FirstOrDefault();

            ((MonitorListViewModel)BindingContext).Monitors.Remove(monitorType);
        }

        public void OnSubcription()
        {
            if (myMonitoredItem != null)
            {
                try
                {
                    myMonitoredItem = opc.RemoveMonitoredItem(mySubscription, myMonitoredItem);
                }
                catch
                {
                    //ignore
                    ;
                }
            }

            try
            {
                string id = "ns=3;s=\"test\"";
                itemCount++;
                string monitoredItemName = "myItem" + itemCount.ToString();
                if (mySubscription == null)
                {
                    mySubscription = opc.Subscribe(2000);
                }
                myMonitoredItem = opc.AddMonitoredItem(mySubscription, id, monitoredItemName, 1);

                opc.ItemChangedNotification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex.ToString());
            }

        }

        #region Notification_Monitor
        int cnt = 0;
        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {

            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            if (notification == null)
            {
                return;
            }
            else
            {
                cnt += 1;
                if (cnt == 4)
                {
                    MonitorType monitorType = new MonitorType();
                    monitorType.Name = monitoredItem.DisplayName;
                    monitorType.Value = Utils.Format("{0}", notification.Value.WrappedValue.ToString());
                    monitorType.SourceT = notification.Value.SourceTimestamp.ToString("hh:mm:ss");
                    monitorType.ServerT = notification.Value.ServerTimestamp.ToString("hh:mm:ss");
                    Monitors.Add(monitorType);
                    cnt = 0;
                }
            }

        }
        #endregion Notification_Monitor


    }
}