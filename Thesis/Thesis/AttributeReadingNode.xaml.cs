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
            ImageUrl.Text = listnode.ImageUrl;
            Value.Text = value;

        }
	}
}