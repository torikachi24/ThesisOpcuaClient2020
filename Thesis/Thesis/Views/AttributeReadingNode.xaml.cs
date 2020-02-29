using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thesis
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AttributeReadingNode
	{
		public AttributeReadingNode (ListNode listnode, string value)
		{
			InitializeComponent ();

            NodeName.Text = listnode.NodeName;
            NodeId.Text = listnode.id;
            NodeClass.Text = listnode.nodeClass;
            AccessLevel.Text = listnode.accessLevel;
            EventNotifier.Text = listnode.eventNotifier;
            Executable.Text = listnode.executable;
            Children.Text = listnode.children.ToString();
            Value.Text = value;

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }
    }
}