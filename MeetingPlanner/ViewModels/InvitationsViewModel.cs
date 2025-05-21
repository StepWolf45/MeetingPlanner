// InvitationsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    public class InvitationsViewModel : ObservableObject
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

            _db.SaveChanges();
            LoadInvitations();

            MessageBox.Show("Response saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public IRelayCommand<EventInvitation> RespondCommand { get; }
    }
}