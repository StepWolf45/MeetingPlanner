using System.Windows.Controls;
using MeetingPlanner.ViewModels;
using System;

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
            if (DataContext is CalendarViewModel viewModel &&
                (sender as Calendar)?.SelectedDate is DateTime selectedDate)
            {
                viewModel.DateSelectedCommand.Execute(selectedDate);
                viewModel.ViewEventsForDate(selectedDate); // Добавьте эту строку
            }
        }
    }
}