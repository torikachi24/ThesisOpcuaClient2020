using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Thesis
{
    public class MonitorListViewModel
    {
        public static string nodeid;
        public static SampleClient opcClient;
        private MonitoredItem myMonitoredItem;
        private Subscription mySubscription;
        private Int16 itemCount;
        public ObservableCollection<MonitorNodeType> Monitors { get; set; }= new ObservableCollection<MonitorNodeType>();
        

        public MonitorListViewModel()
        {
            OnSubcription();
            MessagingCenter.Subscribe<MonitorPage, string>(this, "FlagUnSub",
             (sender, arg) =>
             {

                 opcClient.RemoveSubscription(mySubscription);
                 mySubscription = null;
                 itemCount = 0;
                 opcClient = null;
                 nodeid = null;
             });
        }

        #region Subcription

        public void OnSubcription()
        {
            if (myMonitoredItem != null)
            {
                try
                {
                    myMonitoredItem = opcClient.RemoveMonitoredItem(mySubscription, myMonitoredItem);
                }
                catch
                {
                    //ignore
                    ;
                }
            }

            try
            {

                //use different item names for correct assignment at the notificatino event
                itemCount++;
                string monitoredItemName = "myItem" + itemCount.ToString();
                if (mySubscription == null)
                {
                    mySubscription = opcClient.Subscribe(2000);
                }
                myMonitoredItem = opcClient.AddMonitoredItem(mySubscription, nodeid, monitoredItemName, 1);

                opcClient.ItemChangedNotification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex.ToString());
            }
        }

        #endregion Subcription

        #region Notification_Monitor

        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            if (notification == null)
            {
                return;
            }
            else
            {
                MonitorNodeType monitorType = new MonitorNodeType();
                monitorType.MonitorName += monitoredItem.DisplayName;
                monitorType.MonitorValue += Utils.Format("{0}", notification.Value.WrappedValue.ToString());
                monitorType.MonitorSourceT += notification.Value.SourceTimestamp.ToString("hh:mm:ss");
                monitorType.MonitorServerT += notification.Value.ServerTimestamp.ToString("hh:mm:ss");
                monitorType.MonitorID = Monitors.Count + 1;
                int tmp = 0;
                for (int a = 0; a < Monitors.Count; a++)
                {
                    if (monitorType.MonitorName == Monitors[a].MonitorName)
                    {
                        Monitors.RemoveAt(a);
                        Monitors.Add(monitorType);
                        tmp = 1;
                    }
                }
                if (tmp == 0)
                {
                    Monitors.Add(monitorType);
                }
            }
        }

        #endregion Notification_Monitor

        
        
    }
}