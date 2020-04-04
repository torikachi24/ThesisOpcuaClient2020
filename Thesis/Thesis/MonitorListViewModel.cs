using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Thesis;
using Xamarin.Forms;

namespace Thesis
{
    public class MonitorListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static MonitorType monitor { get; set; }
        private ObservableCollection<MonitorType> _monitors;
        public  ObservableCollection<MonitorType> Monitors
        {
            get
            {
                return _monitors;
            }
            set
            {
                if (value != _monitors)
                {
                    _monitors = value;
                    NotifyPropertyChanged(nameof(Monitors));
                }
            }
        }
        public static MonitorType monitortype;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public MonitorListViewModel()
        {
            Monitors = new ObservableCollection<MonitorType>();

            if (monitor.MonitorId == 0)
            {
                monitor.MonitorId = Monitors.Count + 1;
                Monitors.Add(monitor);
            }
            else
            {
                for (int a = 0; a < Monitors.Count; a++)
                {
                    if (monitor.Name == Monitors[a].Name)
                    {
                        int newIndex = Monitors.IndexOf(monitor);
                        Monitors.Remove(Monitors[a]);
                        Monitors.Add(monitor);
                        int oldIndex = Monitors.IndexOf(Monitors[a]);
                        Monitors.Move(oldIndex, newIndex);


                    }
                    else
                    {
                        Monitors.Add(monitor);
                    }
                }
            }

                //  MessagingCenter.Subscribe<TreeView, MonitorType>(this, "AddOrEditMonitor",
                //(page, monitorType) =>
                //{
                //    if (monitorType.MonitorId == 0)
                //    {
                //        monitorType.MonitorId = Monitors.Count + 1;
                //        Monitors.Add(monitorType);
                //    }
                //    else
                //    {
                //        for (int a = 0; a < Monitors.Count; a++)
                //        {
                //            if (monitorType.Name == Monitors[a].Name)
                //            {
                //                int newIndex = Monitors.IndexOf(monitorType);
                //                Monitors.Remove(Monitors[a]);
                //                Monitors.Add(monitorType);
                //                int oldIndex = Monitors.IndexOf(Monitors[a]);
                //                Monitors.Move(oldIndex, newIndex);


                //            }
                //            else
                //            {
                //                Monitors.Add(monitorType);
                //            }
                //        }
                //        Monitors.Add(monitorType);
                //    }

                //});
            }
    }
}
