using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
        private bool _isEventFormVisible;
        private bool _isEventDetailsVisible;
        private User _selectedFriend;
        private CalendarEvent _selectedEvent;

        public ObservableCollection<CalendarEvent> Events { get; } = new ObservableCollection<CalendarEvent>();
        public ObservableCollection<User> Friends { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> SelectedAttendees { get; } = new ObservableCollection<User>();
        public ObservableCollection<TimeSpan> TimeSlots { get; } = new ObservableCollection<TimeSpan>();

        public CalendarEvent NewEvent { get; } = new CalendarEvent();

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    OnPropertyChanged(nameof(EventsForSelectedDate));
                }
            }
        }

        public bool IsEventFormVisible
        {
            get => _isEventFormVisible;
            set => SetProperty(ref _isEventFormVisible, value);
        }

        public bool IsEventDetailsVisible
        {
            get => _isEventDetailsVisible;
            set => SetProperty(ref _isEventDetailsVisible, value);
        }

        public User SelectedFriend
        {
            get => _selectedFriend;
            set => SetProperty(ref _selectedFriend, value);
        }

        public CalendarEvent SelectedEvent
        {
            get => _selectedEvent;
            set => SetProperty(ref _selectedEvent, value);
        }

        public DateTime StartDate
        {
            get => NewEvent.StartTime.Date;
            set => NewEvent.StartTime = value.Date + NewEvent.StartTime.TimeOfDay;
        }

        public TimeSpan StartTime
        {
            get => NewEvent.StartTime.TimeOfDay;
            set => NewEvent.StartTime = NewEvent.StartTime.Date + value;
        }

        public DateTime EndDate
        {
            get => NewEvent.EndTime.Date;
            set => NewEvent.EndTime = value.Date + NewEvent.EndTime.TimeOfDay;
        }

        public TimeSpan EndTime
        {
            get => NewEvent.EndTime.TimeOfDay;
            set => NewEvent.EndTime = NewEvent.EndTime.Date + value;
        }

        public IEnumerable<CalendarEvent> EventsForSelectedDate =>
            GetEventsForDate(SelectedDate);

        public ICommand CreateEventCommand { get; }
        public ICommand DateSelectedCommand { get; }
        public ICommand AddAttendeeCommand { get; }
        public ICommand RemoveAttendeeCommand { get; }
        public ICommand ViewEventCommand { get; }
        public ICommand CloseEventDetailsCommand { get; }

        public CalendarViewModel(DatabaseService db)
        {
            _db = db;

            CreateEventCommand = new RelayCommand(CreateEvent);
            DateSelectedCommand = new RelayCommand<DateTime?>(OnDateSelected);
            AddAttendeeCommand = new RelayCommand(AddAttendee);
            RemoveAttendeeCommand = new RelayCommand<User>(RemoveAttendee);
            ViewEventCommand = new RelayCommand<DateTime?>(ViewEventsForDate);
            CloseEventDetailsCommand = new RelayCommand(() => IsEventDetailsVisible = false);
            SelectedDate = DateTime.Today;
            InitializeTimeSlots();
        }

        private void InitializeTimeSlots()
        {
            for (var hour = 0; hour < 24; hour++)
            {
                TimeSlots.Add(new TimeSpan(hour, 0, 0));
                TimeSlots.Add(new TimeSpan(hour, 30, 0));
            }
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _currentUser.Friends = _currentUser.Friends ?? new List<User>();
            LoadEvents();
            LoadFriends();
        }

        private void OnDateSelected(DateTime? date)
        {
            if (date.HasValue)
            {
                SelectedDate = date.Value;
                NewEvent.StartTime = date.Value;
                NewEvent.EndTime = date.Value.AddHours(1);
                IsEventFormVisible = true;
                IsEventDetailsVisible = false;
            }
            else
            {
                IsEventFormVisible = false;
            }
        }

        public void ViewEventsForDate(DateTime? date)
        {
            if (!date.HasValue) return;

            var events = GetEventsForDate(date.Value).ToList();
            if (events.Any())
            {
                SelectedEvent = events.First();
                IsEventDetailsVisible = true;
                IsEventFormVisible = false;
            }
        }

        public bool HasEventsOnDate(DateTime date)
        {
            return Events.Any(e => e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date);
        }

        public IEnumerable<CalendarEvent> GetEventsForDate(DateTime date)
        {
            return Events.Where(e => e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date);
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
            OnPropertyChanged(nameof(Events));
            OnPropertyChanged(nameof(EventsForSelectedDate));
        }

        private void LoadFriends()
        {
            Friends.Clear();
            foreach (var friend in _currentUser.Friends.Where(f => f != null))
            {
                Friends.Add(friend);
            }
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
                LoadEvents();

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
    }
}