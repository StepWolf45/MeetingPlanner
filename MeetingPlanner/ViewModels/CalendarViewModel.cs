using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity; // Для использования метода Include
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MeetingPlanner.ViewModels
{
    public class CalendarViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;
        private DateTime _selectedDate;
        private bool _isEventDetailsVisible;
        private User _selectedFriend;

        public ObservableCollection<CalendarEvent> SelectedEvents { get; } = new ObservableCollection<CalendarEvent>();
        public ObservableCollection<CalendarEvent> Events { get; } = new ObservableCollection<CalendarEvent>();
        public ObservableCollection<User> Friends { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> SelectedAttendees { get; } = new ObservableCollection<User>();
        public ObservableCollection<TimeSpan> TimeSlots { get; } = new ObservableCollection<TimeSpan>();

        public CalendarEvent NewEvent { get; } = new CalendarEvent();

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public bool IsEventDetailsVisible
        {
            get => _isEventDetailsVisible;
            set => SetProperty(ref _isEventDetailsVisible, value);
        }
        private CalendarEvent _selectedEvent;
        public CalendarEvent SelectedEvent
        {
            get => _selectedEvent;
            set => SetProperty(ref _selectedEvent, value);
        }

        public User SelectedFriend
        {
            get => _selectedFriend;
            set => SetProperty(ref _selectedFriend, value);
        }

        public CalendarViewModel(DatabaseService db)
        {
            _db = db;
            InitializeTimeSlots();

            CreateEventCommand = new RelayCommand(CreateEvent);
            DateSelectedCommand = new RelayCommand<DateTime?>(OnDateSelected);
            CloseEventDetailsCommand = new RelayCommand(CloseEventDetails);
            AddAttendeeCommand = new RelayCommand(AddAttendee);
            RemoveAttendeeCommand = new RelayCommand<User>(RemoveAttendee);

            SelectedDate = DateTime.Today;
        }

        private void InitializeTimeSlots()
        {
            for (var hour = 8; hour < 20; hour++)
            {
                TimeSlots.Add(new TimeSpan(hour, 0, 0));
                TimeSlots.Add(new TimeSpan(hour, 30, 0));
            }
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = _db.Users
                .Include(u => u.Friends)  // Make sure to add using Microsoft.EntityFrameworkCore;
                .FirstOrDefault(u => u.Id == currentUser.Id);
            LoadEvents();
            LoadFriends();
        }

        private void OnDateSelected(DateTime? date)
        {
            if (!date.HasValue) return;

            SelectedDate = date.Value;
            LoadEventsForSelectedDate();
        }
        private void LoadEventsForSelectedDate()
        {
            var eventsOnDate = GetEventsForDate(SelectedDate).ToList();
            SelectedEvents.Clear();

            foreach (var ev in eventsOnDate)
            {
                // Загружаем связанные данные для каждого события
                var fullEvent = _db.CalendarEvents
                    .Include(e => e.Organizer)
                    .Include(e => e.Attendees)
                    .Include(e => e.Invitations)
                    .FirstOrDefault(e => e.Id == ev.Id);

                SelectedEvents.Add(fullEvent ?? ev);
            }

            if (SelectedEvents.Any())
            {
                SelectedEvent = SelectedEvents.First();
                if (SelectedEvent != null && SelectedEvent.Attendees != null)
                {
                    foreach (var attendee in SelectedEvent.Attendees)
                    {
                        attendee.CurrentEventStatus = GetInvitationStatus(SelectedEvent, attendee.Id);
                    }
                }
                IsEventDetailsVisible = true;
                OnPropertyChanged(nameof(CurrentInvitationStatus)); // Обновляем статус текущего пользователя
            }
            else
            {
                SelectedEvent = null;
                IsEventDetailsVisible = false;
            }

            NewEvent.StartTime = SelectedDate;
            NewEvent.EndTime = SelectedDate.AddHours(1);
        }

        private void CreateEvent()
        {
            if (string.IsNullOrWhiteSpace(NewEvent.Title))
            {
                MessageBox.Show("Please enter event title", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewEvent.EndTime <= NewEvent.StartTime)
            {
                MessageBox.Show("End time must be after start time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                NewEvent.Organizer = _currentUser;
                NewEvent.Attendees = SelectedAttendees.ToList();

                _db.CalendarEvents.Add(NewEvent);

                // Создаем приглашения для всех участников
                foreach (var attendee in SelectedAttendees)
                {
                    var invitation = new EventInvitation
                    {
                        Event = NewEvent,
                        User = attendee,
                        Status = ResponseStatus.Pending
                    };
                    _db.EventInvitations.Add(invitation);
                }

                _db.SaveChanges();
                Events.Add(NewEvent);
                ResetEventForm();

                MessageBox.Show("Событие создано успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetEventForm()
        {
            NewEvent.Title = string.Empty;
            NewEvent.Description = string.Empty;
            NewEvent.Location = string.Empty;
            NewEvent.StartTime = SelectedDate;
            NewEvent.EndTime = SelectedDate.AddHours(1);
            SelectedAttendees.Clear();
        }

        public IEnumerable<CalendarEvent> GetEventsForDate(DateTime date)
        {
            return Events.Where(e => e.StartTime.Date == date.Date)
                        .OrderBy(e => e.StartTime);
        }

         public void LoadEvents()
        {
            Events.Clear();
            var events = _db.CalendarEvents
                .Include(e => e.Organizer)
                .Include(e => e.Attendees)
                .Where(e => e.Organizer.Id == _currentUser.Id ||
                           e.Attendees.Any(a => a.Id == _currentUser.Id))
                .ToList();

            foreach (var ev in events)
            {
                Events.Add(ev);
            }
        }

        private void LoadFriends()
        {

            Friends.Clear();
            foreach (var friend in _currentUser.Friends.Where(f => f != null))
            {
                Friends.Add(friend);
            }
        }

        private void CloseEventDetails()
        {
            IsEventDetailsVisible = false;
        }

        private void AddAttendee()
        {
            if (SelectedFriend != null && !SelectedAttendees.Contains(SelectedFriend))
            {
                SelectedAttendees.Add(SelectedFriend);
            }
        }

        private void RemoveAttendee(User attendee)
        {
            SelectedAttendees.Remove(attendee);
        }

        public ICommand CreateEventCommand { get; }
        public ICommand DateSelectedCommand { get; }
        public ICommand CloseEventDetailsCommand { get; }
        public ICommand AddAttendeeCommand { get; }
        public ICommand RemoveAttendeeCommand { get; }
        // CalendarViewModel.cs
        public string CurrentInvitationStatus
        {
            get
            {
                if (SelectedEvent == null || _currentUser == null)
                    return string.Empty;

                var invitation = _db.EventInvitations
                    .FirstOrDefault(i => i.EventId == SelectedEvent.Id && i.UserId == _currentUser.Id);

                if (invitation == null) return "Не отвечено";

                switch (invitation.Status)
                {
                    case ResponseStatus.Accepted: return "Вы придете";
                    case ResponseStatus.Declined: return "Вы не придете";
                    case ResponseStatus.Maybe: return "Вы возможно придете";
                    case ResponseStatus.Custom: return "Ваш ответ: " + (invitation.CustomResponse ?? "Свой вариант");
                    default: return "Вы не ответили";
                }
            }
        }
        public string GetInvitationStatus(CalendarEvent calendarEvent, int userId)
        {
            if (calendarEvent == null)
            {
                return "Не отвечено";
            }

            // Получаем приглашение из базы данных
            var invitation = _db.EventInvitations
                .FirstOrDefault(i => i.EventId == calendarEvent.Id && i.UserId == userId);

            // Проверяем, является ли текущий пользователь организатором события
            bool isOrganizer = calendarEvent.Organizer?.Id == _currentUser.Id;

            if (isOrganizer)
            {
                // Логика для организатора
                if (invitation == null)
                {
                    return "Не отвечено";
                }

                switch (invitation.Status)
                {
                    case ResponseStatus.Accepted:
                        return "Придет";
                    case ResponseStatus.Declined:
                        return "Не придет";
                    case ResponseStatus.Maybe:
                        return "Возможно";
                    case ResponseStatus.Custom:
                        return !string.IsNullOrEmpty(invitation.CustomResponse)
                            ? invitation.CustomResponse
                            : "Свой вариант";
                    default:
                        return "Не отвечено";
                }
            }
            else
            {
                // Логика для участника
                if (invitation == null)
                {
                    return "Вы не ответили";
                }

                switch (invitation.Status)
                {
                    case ResponseStatus.Accepted:
                        return "Вы придете";
                    case ResponseStatus.Declined:
                        return "Вы не придете";
                    case ResponseStatus.Maybe:
                        return "Вы возможно придете";
                    case ResponseStatus.Custom:
                        return "Ваш ответ: " + (!string.IsNullOrEmpty(invitation.CustomResponse)
                            ? invitation.CustomResponse
                            : "Свой вариант");
                    default:
                        return "Вы не ответили";
                }
            }
        }
    }
}