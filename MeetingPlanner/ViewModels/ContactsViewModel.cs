using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    public class ContactsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private ObservableCollection<User> _searchResults;
        public ObservableCollection<User> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        private ObservableCollection<User> _friends;
        public ObservableCollection<User> Friends
        {
            get => _friends;
            set => SetProperty(ref _friends, value);
        }

        private ObservableCollection<FriendRequest> _pendingRequests;
        public ObservableCollection<FriendRequest> PendingRequests
        {
            get => _pendingRequests;
            set => SetProperty(ref _pendingRequests, value);
        }

        public ContactsViewModel(DatabaseService db)
        {
            _db = db;
            SearchCommand = new RelayCommand(SearchUsers);
            SendFriendRequestCommand = new RelayCommand<User>(SendFriendRequest);
            AcceptFriendRequestCommand = new RelayCommand<FriendRequest>(AcceptFriendRequest);
            DeclineFriendRequestCommand = new RelayCommand<FriendRequest>(DeclineFriendRequest);
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser;
            LoadFriends();
            LoadPendingRequests();
        }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand<User> SendFriendRequestCommand { get; }
        public IRelayCommand<FriendRequest> AcceptFriendRequestCommand { get; }
        public IRelayCommand<FriendRequest> DeclineFriendRequestCommand { get; }

        private void SearchUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults = new ObservableCollection<User>();
                return;
            }

            var results = _db.Users
                .Where(u => (u.Username.Contains(SearchQuery) || u.FirstName.Contains(SearchQuery) || u.LastName.Contains(SearchQuery)) && u.Id != _currentUser.Id)
                .ToList();

            SearchResults = new ObservableCollection<User>(results);
        }

        private void SendFriendRequest(User user)
        {
            if (user.Id == _currentUser.Id)
            {
                MessageBox.Show("You cannot send a friend request to yourself.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingRequest = _db.FriendRequests
                .FirstOrDefault(fr => (fr.SenderId == _currentUser.Id && fr.ReceiverId == user.Id) || (fr.SenderId == user.Id && fr.ReceiverId == _currentUser.Id));

            if (existingRequest != null)
            {
                MessageBox.Show("Friend request already sent or received.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var friendRequest = new FriendRequest
            {
                SenderId = _currentUser.Id,
                ReceiverId = user.Id,
                IsAccepted = false
            };

            _db.FriendRequests.Add(friendRequest);
            _db.SaveChanges();

            MessageBox.Show("Friend request sent!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AcceptFriendRequest(FriendRequest request)
        {
            request.IsAccepted = true;
            _db.SaveChanges();

            _currentUser.Friends.Add(request.Sender);
            request.Sender.Friends.Add(_currentUser);
            _db.SaveChanges();

            LoadFriends();
            LoadPendingRequests();

            MessageBox.Show("Friend request accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeclineFriendRequest(FriendRequest request)
        {
            _db.FriendRequests.Remove(request);
            _db.SaveChanges();

            LoadPendingRequests();

            MessageBox.Show("Friend request declined!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadFriends()
        {
            Friends = new ObservableCollection<User>(_currentUser.Friends);
        }

        private void LoadPendingRequests()
        {
            PendingRequests = new ObservableCollection<FriendRequest>(_db.FriendRequests.Where(fr => fr.ReceiverId == _currentUser.Id && !fr.IsAccepted));
        }
    }
}