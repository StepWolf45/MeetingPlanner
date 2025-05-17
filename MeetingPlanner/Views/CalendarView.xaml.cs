using System.Windows;
using System.Windows.Controls;

namespace MeetingPlanner.Views
{
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendar = sender as System.Windows.Controls.Calendar;
            if (calendar.SelectedDate.HasValue)
            {
                var viewModel = DataContext as ViewModels.CalendarViewModel;
                viewModel?.DateSelectedCommand.Execute(calendar.SelectedDate);
            }
        }
    }
}