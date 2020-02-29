﻿using Rg.Plugins.Popup.Services;
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
        private ObservableCollection<ListNode> nodes = new ObservableCollection<ListNode>();
        private SampleClient opcClient;
        private Tree storedTree;

        public TreeView(Tree tree, SampleClient client)
        {
            InitializeComponent();
            BindingContext = nodes;

            storedTree = tree;
            opcClient = client;
            DisplayNodes();
        }

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

        public async void OnRead(object sender, EventArgs e)
        {
            try
            {
                TappedEventArgs tappedEventArgs = (TappedEventArgs)e;

                var menu = sender as MenuItem;

                var selected = menu.CommandParameter as ListNode;
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
            var menu = sender as MenuItem;
            var selected = menu.CommandParameter as ListNode;

            await PopupNavigation.Instance.PushAsync(new WritePopup(opcClient,selected));
            
        }

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
                    //viewCell.ContextActions.Add(new MenuItem() {Text = "Read"});

                    viewCell.ContextActions.Add(WNode);
                    viewCell.ContextActions.Add(RNode);

                    foreach (var action in viewCell.ContextActions)
                    {
                        action.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                        RNode.Clicked += OnRead;
                        WNode.Clicked += OnWrite;
                    }
                }
            }
        }

        
    }
}