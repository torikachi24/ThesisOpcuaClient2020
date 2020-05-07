using Opc.Ua;
using Opc.Ua.Client;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        #endregion Fields

        #region TreeView

        public TreeView(Tree tree, SampleClient client)
        {
            InitializeComponent();
            BindingContext = nodes;
            storedTree = tree;
            opcClient = client;
            DisplayNodes();
            
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
            string id = "ns=3;s=\"Z\"";
            MonitorListViewModel.opcClient = opcClient;
            MonitorListViewModel.nodeid = id;
         
        }

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
        
        private async void ToolbarItem_Clicked_About(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutPage());
        }

        private async void ToolbarItem_Clicked_Help(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new HelpPage());
        }
    }
}