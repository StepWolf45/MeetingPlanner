// InvitationsViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;

namespace MeetingPlanner.ViewModels
{
    public partial class InvitationsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;

        public ObservableCollection<EventInvitation> PendingInvitations { get; } = new ObservableCollection<EventInvitation>();

        public InvitationsViewModel(DatabaseService db)
        {
            _db = db;
            RespondCommand = new RelayCommand<EventInvitation>(RespondToInvitation);
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser;
            LoadInvitations();
        }

        private void LoadInvitations()
        {
            PendingInvitations.Clear();

            var invitations = _db.EventInvitations
                .Include("Event")
                .Include("Event.Organizer")
                .Where(i => i.UserId == _currentUser.Id && i.Status == ResponseStatus.Pending)
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
            if (button != null)
            {
                switch (button.Tag as string)
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
                    default:
                        invitation.Status = ResponseStatus.Pending;
                        break;
                }

                _db.SaveChanges();
                LoadInvitations();

                MessageBox.Show("Response saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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