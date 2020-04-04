using Opc.Ua;
using Opc.Ua.Client;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Thesis;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeView : TabbedPage
    {
        #region Fields

        private ObservableCollection<ListNode> nodes = new ObservableCollection<ListNode>();
        private SampleClient opcClient;
        private Tree storedTree;
        private MonitoredItem myMonitoredItem;
        private Int16 itemCount;
        private Subscription mySubscription;

        #endregion Fields

        #region TreeView

        public TreeView(Tree tree, SampleClient client)
        {
            InitializeComponent();
            BindingContext = nodes;

            storedTree = tree;
            opcClient = client;
            DisplayNodes();
            itemCount = 0;
        }

        #endregion TreeView

        #region DisplayNodes

        private void DisplayNodes()
        {
            nodes.Clear();

            foreach (var node in storedTree.currentView)
            {
                nodes.Add(node);
            }

            //defined in XAML to follow
            treeView.ItemsSource = null;
            treeView.ItemsSource = nodes;
        }

        private async void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            treeView.SelectedItem = null; // deselect row
            ListNode selected = e.SelectedItem as ListNode;

            if (selected.children == true)
            {
                storedTree = opcClient.GetChildren(selected.id);

                Page treeViewPage = new TreeView(storedTree, opcClient);
                treeViewPage.Title = this.Title + "/" + selected.NodeName;
                await Navigation.PushAsync(treeViewPage);
            }
        }

        #endregion DisplayNodes

        #region Read/Write

        public async void OnRead(object sender, EventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                var selected = menu.CommandParameter as ListNode;
                var value = opcClient.VariableRead(selected.id);
                List<string> datatype = new List<string>();
                VariableNode variablenode = new VariableNode();
                opcClient.Read_Datatype(opcClient, selected.id, out datatype, out variablenode);
                await PopupNavigation.Instance.PushAsync(new AttributeReadingNode(selected, value, datatype[0], variablenode));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void OnWrite(object sender, EventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                var selected = menu.CommandParameter as ListNode;
                List<string> datatype = new List<string>();
                VariableNode variablenode = new VariableNode();
                opcClient.Read_Datatype(opcClient, selected.id, out datatype, out variablenode);
                await PopupNavigation.Instance.PushAsync(new WritePopup(opcClient, selected, datatype[0]));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Read/Write

        #region Subcription

        public void OnSubcription(object sender, EventArgs e)
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
                string id = "ns=3;s=\"Z\"";
                //use different item names for correct assignment at the notificatino event
                itemCount++;
                string monitoredItemName = "myItem" + itemCount.ToString();
                if (mySubscription == null)
                {
                    mySubscription = opcClient.Subscribe(2000);
                }
                myMonitoredItem = opcClient.AddMonitoredItem(mySubscription, id, monitoredItemName, 1);

                opcClient.ItemChangedNotification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
                //Navigation.PushAsync(new TabbedPageMonitor());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error"+ex.ToString());
            }
           
        }

        //private void UnsubscribeButton_Click(object sender, EventArgs e)
        //{
        //    opcClient.RemoveSubscription(mySubscription);
        //    mySubscription = null;
        //    itemCount = 0;
        //}
        #endregion Subcription

        #region Graph

        private void OnGraph(object sender, EventArgs e)
        {
            var menu = sender as MenuItem;
            var selected = menu.CommandParameter as ListNode;
            string id = "ns=3;s=\"test\"";
            BindingContext = new ChartVM(opcClient, id);
            CurrentPage = Children[1];
        }

        #endregion Graph

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
                //Console.WriteLine(cnt);
                if (cnt == 4)
                {
                    MonitorType monitorType = new MonitorType();
                monitorType.Name = monitoredItem.DisplayName;
                monitorType.Value = Utils.Format("{0}", notification.Value.WrappedValue.ToString());
                monitorType.SourceT = notification.Value.SourceTimestamp.ToString("hh:mm:ss");
                monitorType.ServerT = notification.Value.ServerTimestamp.ToString("hh:mm:ss");
                MonitorListViewModel.monitor = monitorType;
                    //MessagingCenter.Send<TreeView, MonitorType>(this, "AddOrEditMonitor", monitorType);
                 cnt = 0;
                }
            }

        }

        #endregion Notification_Monitor

        #region MenuItems

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();

            if (BindingContext == null)
            {
                return;
            }

            ViewCell viewCell = sender as ViewCell;
            var item = viewCell.BindingContext as ListNode;
            viewCell.ContextActions.Clear();

            if (item != null)
            {
                if (item.nodeClass == "Variable")
                {
                    MenuItem WNode = new MenuItem { Text = "Write" };
                    MenuItem RNode = new MenuItem { Text = "Read" };
                    MenuItem Graph = new MenuItem { Text = "Graph" };
                    MenuItem Sub = new MenuItem { Text = "Sub" };
                    //viewCell.ContextActions.Add(new MenuItem() {Text = "Read"});

                    viewCell.ContextActions.Add(WNode);
                    viewCell.ContextActions.Add(RNode);
                    viewCell.ContextActions.Add(Graph);
                    viewCell.ContextActions.Add(Sub);
                    foreach (var action in viewCell.ContextActions)
                    {
                        action.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                        RNode.Clicked += OnRead;
                        WNode.Clicked += OnWrite;
                        Sub.Clicked += OnSubcription;
                        Graph.Clicked += OnGraph;
                    }
                }
            }
        }

        #endregion MenuItems
    }
}