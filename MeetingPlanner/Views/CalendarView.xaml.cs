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
            if (DataContext is ViewModels.CalendarViewModel viewModel)
            {
                viewModel.DateSelectedCommand.Execute((sender as Calendar)?.SelectedDate);
            }
        }
    }
}