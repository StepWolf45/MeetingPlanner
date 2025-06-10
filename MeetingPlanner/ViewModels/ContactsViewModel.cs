using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MeetingPlanner.ViewModels
{
    public class ContactsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;
        private string _searchQuery;
        private ObservableCollection<User> _searchResults;
        private ObservableCollection<User> _friends;
        private ObservableCollection<FriendRequest> _pendingRequests;
        private User _selectedUser;
        private bool _isTagPopupOpen;
        private string _currentTagText;
        private string _selectedTagColor;
        private FriendRequest _currentFriendRequest;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public ObservableCollection<User> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public ObservableCollection<User> Friends
        {
            get => _friends;
            set => SetProperty(ref _friends, value);
        }

        public ObservableCollection<FriendRequest> PendingRequests
        {
            get => _pendingRequests;
            set => SetProperty(ref _pendingRequests, value);
        }

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

        public bool IsTagPopupOpen
        {
            get => _isTagPopupOpen;
            set
            {
                SetProperty(ref _isTagPopupOpen, value);
                if (value)
                {
                    CurrentTagText = string.Empty;
                    SelectedTagColor = AvailableTagColors.FirstOrDefault();
                    RequestFocus?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string CurrentTagText
        {
            get => _currentTagText;
            set => SetProperty(ref _currentTagText, value);
        }

        public string SelectedTagColor
        {
            get => _selectedTagColor;
            set => SetProperty(ref _selectedTagColor, value);
        }

        public FriendRequest CurrentFriendRequest
        {
            get => _currentFriendRequest;
            set => SetProperty(ref _currentFriendRequest, value);
        }

        public ObservableCollection<string> AvailableTagColors { get; } = new ObservableCollection<string>
        {
            "#FF5733", "#33FF57", "#3357FF", "#F333FF", "#33FFF3"
        };

        public event EventHandler RequestFocus;

        public ICommand SearchCommand { get; }
        public ICommand SendFriendRequestCommand { get; }
        public ICommand AcceptFriendRequestCommand { get; }
        public ICommand DeclineFriendRequestCommand { get; }
        public ICommand SaveFriendTagCommand { get; }

        public string FriendStatus => SelectedUser != null ? GetFriendStatus(SelectedUser) : string.Empty;
        public Brush FriendStatusColor => SelectedUser != null ? GetFriendStatusColor(SelectedUser) : Brushes.Black;

        public ContactsViewModel(DatabaseService db)
        {
            _db = db;
            SearchResults = new ObservableCollection<User>();
            Friends = new ObservableCollection<User>();
            PendingRequests = new ObservableCollection<FriendRequest>();

            SearchCommand = new RelayCommand(SearchUsers);
            SendFriendRequestCommand = new RelayCommand<User>(SendFriendRequest);
            AcceptFriendRequestCommand = new RelayCommand<FriendRequest>(AcceptFriendRequest);
            DeclineFriendRequestCommand = new RelayCommand<FriendRequest>(DeclineFriendRequest);
            SaveFriendTagCommand = new RelayCommand(SaveFriendTag);
        }
        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser;
            LoadFriends();
            LoadPendingRequests();
        }
        public Func<User, FriendTag> GetFriendTag => (friend) =>
        {
            if (CurrentUser == null || friend == null) return null;
            return _db.FriendTags.FirstOrDefault(ft =>
                ft.OwnerId == CurrentUser.Id && ft.FriendId == friend.Id);
        };

        private async void SaveFriendTag()
        {
            if (CurrentFriendRequest == null || string.IsNullOrWhiteSpace(CurrentTagText))
            {
                MessageBox.Show("Пожалуйста, введите тег", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sender = _db.Users.FirstOrDefault(u => u.Id == CurrentFriendRequest.SenderId);
            var receiver = _db.Users.FirstOrDefault(u => u.Id == CurrentFriendRequest.ReceiverId);

            if (sender == null || receiver == null)
            {
                MessageBox.Show("Ошибка: пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            var existingTagForCurrentUser = _db.FriendTags.FirstOrDefault(ft =>
                ft.OwnerId == CurrentUser.Id && ft.FriendId == sender.Id);

            var existingTagForFriend = _db.FriendTags.FirstOrDefault(ft =>
                ft.OwnerId == sender.Id && ft.FriendId == CurrentUser.Id);

            if (existingTagForCurrentUser != null)
            {
                existingTagForCurrentUser.TagName = CurrentTagText;
                existingTagForCurrentUser.TagColor = SelectedTagColor;
            }
            else
            {
                _db.FriendTags.Add(new FriendTag
                {
                    OwnerId = CurrentUser.Id,
                    FriendId = sender.Id,
                    TagName = CurrentTagText,
                    TagColor = SelectedTagColor
                });
            }

            if (existingTagForFriend != null)
            {
                existingTagForFriend.TagName = CurrentTagText;
                existingTagForFriend.TagColor = SelectedTagColor;
            }
            else
            {
                _db.FriendTags.Add(new FriendTag
                {
                    OwnerId = sender.Id,
                    FriendId = CurrentUser.Id,
                    TagName = CurrentTagText,
                    TagColor = SelectedTagColor
                });
            }


            if (!CurrentUser.Friends.Any(f => f.Id == sender.Id))
            {
                CurrentUser.Friends.Add(sender);
            }

            if (!sender.Friends.Any(f => f.Id == CurrentUser.Id))
            {
                sender.Friends.Add(CurrentUser);
            }


            await _db.SaveChangesAsync();
            IsTagPopupOpen = false;

     
            LoadFriends();
            LoadPendingRequests();
            RefreshSearchResults();

            MessageBox.Show("Тег успешно сохранён и пользователь добавлен в друзья!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                return;
            }

            var results = _db.Users
                .Where(u => (u.Username.Contains(SearchQuery) ||
                            u.FirstName.Contains(SearchQuery) ||
                            u.LastName.Contains(SearchQuery)) &&
                            u.Id != CurrentUser.Id)
                .ToList();

            foreach (var user in results)
            {
                UpdateUserFriendStatus(user);
            }

            SearchResults = new ObservableCollection<User>(results);
        }
        private async void SendFriendRequest(User user)
        {
            if (user.Id == CurrentUser.Id)
            {
                MessageBox.Show("Нельзя отправить запрос самому себе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingRequest = _db.FriendRequests
                .FirstOrDefault(fr => (fr.SenderId == CurrentUser.Id && fr.ReceiverId == user.Id) ||
                                    (fr.SenderId == user.Id && fr.ReceiverId == CurrentUser.Id));

            if (existingRequest != null)
            {
                MessageBox.Show("Запрос уже отправлен или получен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var friendRequest = new FriendRequest
            {
                SenderId = CurrentUser.Id,
                ReceiverId = user.Id,
                IsAccepted = false
            };

            _db.FriendRequests.Add(friendRequest);
            await _db.SaveChangesAsync();

            UpdateUserFriendStatus(user);
            RefreshSearchResults();
        }

        private void AcceptFriendRequest(FriendRequest request)
        {
            request.IsAccepted = true;
            CurrentFriendRequest = request;
            IsTagPopupOpen = true;
            _db.SaveChanges();

            CurrentTagText = "Друг"; 
            SelectedTagColor = AvailableTagColors.FirstOrDefault();
        }

        private void DeclineFriendRequest(FriendRequest request)
        {
            _db.FriendRequests.Remove(request);
            _db.SaveChanges();
            LoadPendingRequests();
        }

        private void LoadFriends()
        {
            Friends = new ObservableCollection<User>(CurrentUser.Friends ?? new List<User>());
        }

        private void LoadPendingRequests()
        {
            PendingRequests = new ObservableCollection<FriendRequest>(
                _db.FriendRequests
                    .Where(fr => fr.ReceiverId == CurrentUser.Id && !fr.IsAccepted)
                    .ToList());
        }

        private void UpdateUserFriendStatus(User user)
        {
            user.FriendStatus = GetFriendStatus(user);
            user.FriendStatusColor = GetFriendStatusColor(user);
        }

        private string GetFriendStatus(User user)
        {
            if (IsFriend(user))
                return "Уже в друзьях";

            if (_db.FriendRequests.Any(fr =>
                (fr.SenderId == CurrentUser.Id && fr.ReceiverId == user.Id) ||
                (fr.SenderId == user.Id && fr.ReceiverId == CurrentUser.Id)))
                return "Запрос отправлен";

            return "Добавить в друзья";
        }

        private Brush GetFriendStatusColor(User user)
        {
            if (IsFriend(user))
                return Brushes.Green;

            if (_db.FriendRequests.Any(fr =>
                (fr.SenderId == CurrentUser.Id && fr.ReceiverId == user.Id) ||
                (fr.SenderId == user.Id && fr.ReceiverId == CurrentUser.Id)))
                return Brushes.Orange;

            return Brushes.Black;
        }

        private bool IsFriend(User user)
        {
            return CurrentUser?.Friends?.Any(f => f.Id == user.Id) ?? false;
        }

        private void RefreshSearchResults()
        {
            if (SearchResults != null)
            {
                var temp = SearchResults.ToList();
                SearchResults = new ObservableCollection<User>(temp);
            }
        }

    }
}