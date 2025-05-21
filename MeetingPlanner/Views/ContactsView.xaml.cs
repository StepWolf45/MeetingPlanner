using System.Windows.Controls;
using MeetingPlanner.ViewModels;

namespace MeetingPlanner.Views
{
    public partial class ContactsView : UserControl
    {
        public ContactsView()
        {
            InitializeComponent();

            if (DataContext is ContactsViewModel vm)
            {
                vm.RequestFocus += (s, e) =>
                {
                    Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        TagInputTextBox?.Focus();
                    }), System.Windows.Threading.DispatcherPriority.Render);
                };
            }
        }
    }
}