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
        private CalendarEvent _selectedEvent;
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
            if (eventsOnDate.Any())
            {
                SelectedEvents.Clear();
                foreach (var ev in eventsOnDate)
                {
                    SelectedEvents.Add(ev);
                }
                IsEventDetailsVisible = true;
            }
            else
            {
                SelectedEvents.Clear();
                IsEventDetailsVisible = false;
            }

            // Сбрасываем форму для нового события
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
                _db.SaveChanges();

                Events.Add(NewEvent);
                ResetEventForm();

                MessageBox.Show("Event created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating event: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void LoadEvents()
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
    }
}