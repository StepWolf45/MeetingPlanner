// InvitationsViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Data.Entity;

namespace MeetingPlanner.ViewModels
{
    public partial class InvitationsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;

        public ObservableCollection<EventInvitation> PendingInvitations { get; } = new ObservableCollection<EventInvitation>();
        private CalendarEvent _selectedInvitationEvent;
        public CalendarEvent SelectedInvitationEvent
        {
            get => _selectedInvitationEvent;
            set => SetProperty(ref _selectedInvitationEvent, value);
        }

        private bool _isInvitationEventVisible;
        public bool IsInvitationEventVisible
        {
            get => _isInvitationEventVisible;
            set => SetProperty(ref _isInvitationEventVisible, value);
        }

        public ICommand ViewEventCommand { get; }



        private void ViewEventDetails(CalendarEvent calendarEvent)
        {
            if (calendarEvent == null) return;

            SelectedInvitationEvent = _db.CalendarEvents
                .Include(e => e.Organizer)
                .Include(e => e.Attendees)
                .Include(e => e.Invitations)
                .FirstOrDefault(e => e.Id == calendarEvent.Id);

            IsInvitationEventVisible = true;
        }
        public InvitationsViewModel(DatabaseService db)
        {
            _db = db;
            RespondCommand = new RelayCommand<EventInvitation>(RespondToInvitation);
            ViewEventCommand = new RelayCommand<CalendarEvent>(ViewEventDetails);
        }
        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser;
            LoadInvitations();
        }
        public ICommand CloseEventDetailsCommand => new RelayCommand(() =>
        {
            IsInvitationEventVisible = false;
        });
        private void LoadInvitations()
        {
            PendingInvitations.Clear();

            var invitations = _db.EventInvitations
                .Include("Event")
                .Include("Event.Organizer")
                .Where(i => i.UserId == _currentUser.Id &&
                       (i.Status == ResponseStatus.Pending || i.Status == ResponseStatus.Maybe))
                .OrderByDescending(i => i.Event.StartTime)
                .ToList();

            foreach (var inv in invitations)
            {
                PendingInvitations.Add(inv);
            }
        }

        private void RespondToInvitation(EventInvitation invitation)
        {
            if (invitation == null) return;

            var button = FindButtonByCommand();
            if (button != null && button.Tag is string tag)
            {
                switch (tag)
                {
                    case "Accepted":
                        invitation.Status = ResponseStatus.Accepted;
                        break;
                    case "Declined":
                        invitation.Status = ResponseStatus.Declined;
                        break;
                    case "Maybe":
                        invitation.Status = ResponseStatus.Maybe;
                        break;
                }

                invitation.ResponseDate = DateTime.Now;
                _db.SaveChanges();

                string responseMessage;
                switch (tag)
                {
                    case "Accepted":
                        responseMessage = "Вы подтвердили участие";
                        break;
                    case "Declined":
                        responseMessage = "Вы отказались от участия";
                        break;
                    case "Maybe":
                        responseMessage = "Вы ответили 'Возможно'";
                        break;
                    default:
                        responseMessage = "Ответ сохранен";
                        break;
                }

                MessageBox.Show(responseMessage, "Спасибо за ответ!", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadInvitations();

                var mainWindow = Application.Current.MainWindow;
                if (mainWindow?.DataContext is HomeViewModel homeViewModel)
                {
                    homeViewModel.CalendarViewModel.LoadEvents();
                }
            }
        }
        private Button FindButtonByCommand()
        {
            foreach (Window window in Application.Current.Windows)
            {
                var button = FindVisualChild<Button>(window, b => b.Command == RespondCommand);
                if (button != null) return button;
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && predicate(result))
                {
                    return result;
                }
                var childResult = FindVisualChild(child, predicate);
                if (childResult != null) return childResult;
            }
            return null;
        }

        public IRelayCommand<EventInvitation> RespondCommand { get; }
    }
}