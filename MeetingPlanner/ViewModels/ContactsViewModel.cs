using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(FriendStatus));
                OnPropertyChanged(nameof(FriendStatusColor));
            }
        }

        public string FriendStatus =>
            SelectedUser != null ? GetFriendStatus(SelectedUser) : string.Empty;

        public Brush FriendStatusColor =>
            SelectedUser != null ? GetFriendStatusColor(SelectedUser) : Brushes.Black;
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
                .Where(u => (u.Username.Contains(SearchQuery) ||
                            u.FirstName.Contains(SearchQuery) ||
                            u.LastName.Contains(SearchQuery)) &&
                            u.Id != _currentUser.Id)
                .ToList();

            // Обновляем статусы для каждого пользователя
            foreach (var user in results)
            {
                UpdateUserFriendStatus(user);
            }

            SearchResults = new ObservableCollection<User>(results);
        }
        private void UpdateUserFriendStatus(User user)
        {
            user.FriendStatus = GetFriendStatus(user);
            user.FriendStatusColor = GetFriendStatusColor(user);
        }

        private async void SendFriendRequest(User user)
        {
            if (user.Id == _currentUser.Id)
            {
                MessageBox.Show("Нельзя отправить запрос самому себе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingRequest = _db.FriendRequests
                .FirstOrDefault(fr => (fr.SenderId == _currentUser.Id && fr.ReceiverId == user.Id) ||
                                    (fr.SenderId == user.Id && fr.ReceiverId == _currentUser.Id));

            if (existingRequest != null)
            {
                MessageBox.Show("Запрос уже отправлен или получен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var friendRequest = new FriendRequest
            {
                SenderId = _currentUser.Id,
                ReceiverId = user.Id,
                IsAccepted = false
            };

            _db.FriendRequests.Add(friendRequest);
            await _db.SaveChangesAsync();

            // Обновляем список результатов поиска
            var updatedResults = SearchResults.Select(u =>
            {
                if (u.Id == user.Id)
                {
                    // Помечаем, что запрос отправлен
                    u.HasPendingRequest = true;
                }
                return u;
            }).ToList();

            SearchResults = new ObservableCollection<User>(updatedResults);

            MessageBox.Show("Запрос в друзья отправлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateUserFriendStatus(user);
            var temp = SearchResults.ToList();
            SearchResults = new ObservableCollection<User>(temp);
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
            UpdateUserFriendStatus(request.Sender);
            UpdateUserFriendStatus(_currentUser);

            // Принудительно обновляем отображение
            var temp = SearchResults.ToList();
            SearchResults = new ObservableCollection<User>(temp);
        }

        private void DeclineFriendRequest(FriendRequest request)
        {
            _db.FriendRequests.Remove(request);
            _db.SaveChanges();

            LoadPendingRequests();

            MessageBox.Show("Friend request declined!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private string GetFriendStatus(User user)
        {
            if (IsFriend(user))
                return "Уже в друзьях";

            if (_db.FriendRequests.Any(fr =>
                (fr.SenderId == _currentUser.Id && fr.ReceiverId == user.Id) ||
                (fr.SenderId == user.Id && fr.ReceiverId == _currentUser.Id)))
                return "Запрос отправлен";

            return "Добавить в друзья";
        }
        private void LoadFriends()
        {
            Friends = _currentUser.Friends != null
                ? new ObservableCollection<User>(_currentUser.Friends)
                : new ObservableCollection<User>();
        }
        public bool CanSendFriendRequest(User user)
        {
            return !IsFriend(user) &&
                   !_db.FriendRequests.Any(fr =>
                       (fr.SenderId == _currentUser.Id && fr.ReceiverId == user.Id) ||
                       (fr.SenderId == user.Id && fr.ReceiverId == _currentUser.Id));
        }

        private void LoadPendingRequests()
        {
            var requests = _db.FriendRequests
                .Where(fr => fr.ReceiverId == _currentUser.Id && !fr.IsAccepted)
                .ToList();

            PendingRequests = requests != null
                ? new ObservableCollection<FriendRequest>(requests)
                : new ObservableCollection<FriendRequest>();
        }

        public bool IsFriend(User user)
        {
            return _currentUser?.Friends?.Any(f => f.Id == user.Id) ?? false;
        }
        private Brush GetFriendStatusColor(User user)
        {
            if (IsFriend(user))
                return Brushes.Green;

            if (_db.FriendRequests.Any(fr =>
                (fr.SenderId == _currentUser.Id && fr.ReceiverId == user.Id) ||
                (fr.SenderId == user.Id && fr.ReceiverId == _currentUser.Id)))
                return Brushes.Orange;

            return Brushes.Black;
        }
    }
}