using Opc.Ua;
using Opc.Ua.Client;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeView : ContentPage
    {
        #region Fields
        private ObservableCollection<ListNode> nodes = new ObservableCollection<ListNode>();
        private SampleClient opcClient;
        private Tree storedTree;
        private MonitoredItem myMonitoredItem;
        private Int16 itemCount;
        private Subscription mySubscription;
        #endregion

        #region TreeView
        public TreeView(Tree tree, SampleClient client)
        {
            InitializeComponent();
            BindingContext = nodes;

            storedTree = tree;
            opcClient = client;
            DisplayNodes();
        }
        #endregion

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
        #endregion

    

        #region Read/Write
        public async void OnRead(object sender, EventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                var selected = menu.CommandParameter as ListNode;
                //var value = opcClient.VariableRead("ns=3;s=\"Realnum\"");
                //Console.WriteLine(value);
                var value = opcClient.VariableRead(selected.id);

                await PopupNavigation.Instance.PushAsync(new AttributeReadingNode(selected, value));

            }
            catch(Exception ex)
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

                await PopupNavigation.Instance.PushAsync(new WritePopup(opcClient, selected));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            
        }
        #endregion

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
                    ;
                }
            }
            try
            {
                var menu = sender as MenuItem;
                var selected = menu.CommandParameter as ListNode;
                itemCount++;
                string monitoredItemName = "myItem" + itemCount.ToString();
                if (mySubscription == null)
                {
                    mySubscription = opcClient.Subscribe(2000);
                }
                myMonitoredItem = opcClient.AddMonitoredItem(mySubscription, "ns=3;s=\"RealNum\"", monitoredItemName, 1);
                //myMonitoredItem = opcClient.AddMonitoredItem(mySubscription, selected.id.ToString(), monitoredItemName, 1);
                opcClient.ItemChangedNotification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region Notification_Monitor
        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            if (notification == null)
            {
                return;
            }

            //Console.WriteLine(monitoredItem.DisplayName);
            //Console.WriteLine(notification.Value.WrappedValue.ToString());
            //Console.WriteLine(notification.Value.SourceTimestamp.ToString());
            //Console.WriteLine(notification.Value.ServerTimestamp.ToString());
            //subscriptionTextBox.Text = "Item name: " + monitoredItem.DisplayName;
            //subscriptionTextBox.Text += System.Environment.NewLine + "Value: " + Utils.Format("{0}", notification.Value.WrappedValue.ToString());
            //subscriptionTextBox.Text += System.Environment.NewLine + "Source timestamp: " + notification.Value.SourceTimestamp.ToString();
            //subscriptionTextBox.Text += System.Environment.NewLine + "Server timestamp: " + notification.Value.ServerTimestamp.ToString();
        }
        #endregion

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
                    MenuItem SubNode = new MenuItem { Text = "Sub" };
                    //viewCell.ContextActions.Add(new MenuItem() {Text = "Read"});

                    viewCell.ContextActions.Add(WNode);
                    viewCell.ContextActions.Add(RNode);
                    viewCell.ContextActions.Add(SubNode);
                    foreach (var action in viewCell.ContextActions)
                    {
                        action.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                        RNode.Clicked += OnRead;
                        WNode.Clicked += OnWrite;
                        SubNode.Clicked += OnSubcription;
                    }
                }
            }
        }
        #endregion

    }
}