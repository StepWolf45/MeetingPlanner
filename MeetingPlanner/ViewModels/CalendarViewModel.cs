using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

        public CalendarEvent _newEvent = new CalendarEvent();
        public CalendarEvent NewEvent
        {
            get => _newEvent;
            set => SetProperty(ref _newEvent, value);
        }
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }
        public DateTime StartDate
        {
            get => NewEvent.StartTime.Date;
            set
            {
                NewEvent.StartTime = value.Date + NewEvent.StartTime.TimeOfDay;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => NewEvent.EndTime.Date;
            set
            {
                NewEvent.EndTime = value.Date + NewEvent.EndTime.TimeOfDay;
                OnPropertyChanged();
            }
        }

        public TimeSpan StartTime
        {
            get => NewEvent.StartTime.TimeOfDay;
            set
            {
                NewEvent.StartTime = NewEvent.StartTime.Date + value;
                OnPropertyChanged();
            }
        }

        public TimeSpan EndTime
        {
            get => NewEvent.EndTime.TimeOfDay;
            set
            {
                NewEvent.EndTime = NewEvent.EndTime.Date + value;
                OnPropertyChanged();
            }
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
            set
            {
                SetProperty(ref _selectedEvent, value);
                if (value != null)
                {
                    foreach (var attendee in value.Attendees)
                    {
                        attendee.CurrentEventStatus = GetAttendeeStatus(value, attendee);
                    }
                    OnPropertyChanged(nameof(SelectedEvent));

                }
                OnPropertyChanged(nameof(SelectedEvent));
            }
        }

        public User SelectedFriend
        {
            get => _selectedFriend;
            set => SetProperty(ref _selectedFriend, value);
        }
        private int _currentEventIndex;
        public string EventCounterText => $"{_currentEventIndex + 1} из {SelectedEvents.Count}";
        public CalendarViewModel(DatabaseService db)
        {
            Console.WriteLine("CalendarViewModel constructor called");
            _db = db;
            InitializeTimeSlots();

            CreateEventCommand = new RelayCommand(CreateEvent);
            DateSelectedCommand = new RelayCommand<DateTime?>(OnDateSelected);
            CloseEventDetailsCommand = new RelayCommand(CloseEventDetails);
            AddAttendeeCommand = new RelayCommand(AddAttendee);
            RemoveAttendeeCommand = new RelayCommand<User>(RemoveAttendee);

            PreviousEventCommand = new RelayCommand(
                () =>
                {
                    Console.WriteLine($"Previous command executed. Index: {_currentEventIndex}");
                    if (_currentEventIndex > 0)
                    {
                        _currentEventIndex--;
                        SelectedEvent = SelectedEvents[_currentEventIndex];
                        Console.WriteLine($"New index: {_currentEventIndex}, Event: {SelectedEvent?.Title}");
                        OnPropertyChanged(nameof(EventCounterText));
                        (PreviousEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
                        (NextEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    }
                },
                () =>
                {
                    bool canExec = SelectedEvents.Any() && _currentEventIndex > 0;
                    Console.WriteLine($"Previous CanExecute: {canExec} (Count: {SelectedEvents.Count}, Index: {_currentEventIndex})");
                    return canExec;
                }
            );
            NextEventCommand = new RelayCommand(
                () =>
                {
                    Console.WriteLine($"NextEventCommand executed. Current index: {_currentEventIndex}");
                    if (_currentEventIndex < SelectedEvents.Count - 1)
                    {
                        _currentEventIndex++;
                        SelectedEvent = SelectedEvents[_currentEventIndex];
                        Console.WriteLine($"New index: {_currentEventIndex}, Event: {SelectedEvent?.Title}");
                        OnPropertyChanged(nameof(EventCounterText));
                        (PreviousEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
                        (NextEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    }
                },
                () =>
                {
                    bool canExec = SelectedEvents.Any() && _currentEventIndex < SelectedEvents.Count - 1;
                    Console.WriteLine($"Next CanExecute: {canExec} (Count: {SelectedEvents.Count}, Index: {_currentEventIndex})");
                    return canExec;
                }
            );

            SelectedDate = DateTime.Today;
            Console.WriteLine("CalendarViewModel initialized");
        }
        private void InitializeTimeSlots()
        {
            for (var hour = 0; hour < 25; hour++)
            {
                TimeSlots.Add(new TimeSpan(hour, 0, 0));
                TimeSlots.Add(new TimeSpan(hour, 20, 0));
            }
        }
        public string GetAttendeeStatus(CalendarEvent calendarEvent, User attendee)
        {
            if (calendarEvent == null || attendee == null)
            {
                return "Не отвечено";
            }
            if (calendarEvent.Organizer != null && calendarEvent.Organizer.Id == attendee.Id)
            {
                return "Организатор";
            }

            var invitation = _db.EventInvitations
                .Include(i => i.Event)
                .Include(i => i.User)
                .FirstOrDefault(i => i.EventId == calendarEvent.Id && i.UserId == attendee.Id);

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
                    return string.IsNullOrEmpty(invitation.CustomResponse)
                        ? "Свой вариант"
                        : invitation.CustomResponse;
                default:
                    return "Не отвечено";
            }
        }
        public void SetCurrentUser(User currentUser)
        {
            _currentUser = _db.Users
                .Include(u => u.Friends)
                .FirstOrDefault(u => u.Id == currentUser.Id);
            LoadEvents();
            LoadFriends();
        }
        private void OnDateSelected(DateTime? date)
        {
            if (!date.HasValue) return;

            SelectedDate = date.Value;
            StartDate = date.Value;
            EndDate = date.Value;
            SelectedEvents.Clear();
            LoadEventsForSelectedDate();
        }
        public void LoadEventsForSelectedDate()
        {
            var eventsOnDate = GetEventsForDate(SelectedDate).ToList();
            SelectedEvents.Clear();
            foreach (var ev in eventsOnDate.OrderBy(e => e.StartTime))
            {
                SelectedEvents.Add(ev);
            }

            _currentEventIndex = SelectedEvents.Any() ? 0 : -1;
            OnPropertyChanged(nameof(EventCounterText));

            if (SelectedEvents.Any())
            {
                SelectedEvent = SelectedEvents[_currentEventIndex];
                IsEventDetailsVisible = true;
            }
            else
            {
                SelectedEvent = null;
                IsEventDetailsVisible = false;
            }

            (PreviousEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (NextEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }
        public void RefreshCommands()
        {
            (PreviousEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (NextEventCommand as RelayCommand)?.NotifyCanExecuteChanged();
            Console.WriteLine("Commands refreshed");
        }
        private void CreateEvent()
        {
            if (string.IsNullOrWhiteSpace(NewEvent.Title))
            {
                MessageBox.Show("Пожалуйста, введите название события", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewEvent.Location))
            {
                MessageBox.Show("Пожалуйста, укажите локацию события", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewEvent.StartTime.TimeOfDay == TimeSpan.Zero)
            {
                MessageBox.Show("Пожалуйста, укажите время начала события", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewEvent.EndTime.TimeOfDay == TimeSpan.Zero)
            {
                MessageBox.Show("Пожалуйста, укажите время окончания события", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewEvent.EndTime <= NewEvent.StartTime)
            {
                MessageBox.Show("Время окончания должно быть позже времени начала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewEvent.EndTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя создать событие, которое уже полностью завершилось", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (HasOverlappingEvents(NewEvent))
            {
                var overlappingEvents = Events.Where(e =>
                    e.StartTime.Date == NewEvent.StartTime.Date &&
                    e.Id != NewEvent.Id &&
                    ((NewEvent.StartTime >= e.StartTime && NewEvent.StartTime < e.EndTime) ||
                     (NewEvent.EndTime > e.StartTime && NewEvent.EndTime <= e.EndTime) ||
                     (NewEvent.StartTime <= e.StartTime && NewEvent.EndTime >= e.EndTime))
                ).ToList();

                var eventList = string.Join("\n", overlappingEvents.Select(e =>
                    $"{e.Title} ({e.StartTime:HH:mm} - {e.EndTime:HH:mm})"));

                MessageBox.Show($"Событие пересекается с существующими:\n{eventList}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                NewEvent.Organizer = _currentUser;
                NewEvent.Attendees = SelectedAttendees.ToList();

                _db.CalendarEvents.Add(NewEvent);

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
                OnPropertyChanged(nameof(Events));
                LoadEventsForSelectedDate();
                ResetEventForm();

                MessageBox.Show("Событие создано успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool HasOverlappingEvents(CalendarEvent newEvent)
        {
            var sameDayEvents = Events.Where(e =>
                e.StartTime.Date == newEvent.StartTime.Date &&
                e.Id != newEvent.Id).ToList();

            foreach (var existingEvent in sameDayEvents)
            {
                bool startsDuring = newEvent.StartTime >= existingEvent.StartTime &&
                                    newEvent.StartTime < existingEvent.EndTime;

                bool endsDuring = newEvent.EndTime > existingEvent.StartTime &&
                                  newEvent.EndTime <= existingEvent.EndTime;

                bool surrounds = newEvent.StartTime <= existingEvent.StartTime &&
                                 newEvent.EndTime >= existingEvent.EndTime;

                bool isSurrounded = newEvent.StartTime >= existingEvent.StartTime &&
                                    newEvent.EndTime <= existingEvent.EndTime;

                if (startsDuring || endsDuring || surrounds || isSurrounded)
                {
                    return true;
                }
            }

            return false;
        }
        private void ResetEventForm()
        {
            NewEvent = new CalendarEvent
            {
                StartTime = SelectedDate,
                EndTime = SelectedDate.AddHours(1)
            };
            SelectedAttendees.Clear();
        }

        public IEnumerable<CalendarEvent> GetEventsForDate(DateTime date)
        {
            return Events.Where(e => e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date)
                        .OrderBy(e => e.StartTime);
        }
        public void LoadEvents()
        {
            Events.Clear();

            var events = _db.CalendarEvents
                .Include(e => e.Organizer)
                .Include(e => e.Attendees)
                .Include(e => e.Invitations)
                .Where(e => e.Organizer.Id == _currentUser.Id ||
                           e.Attendees.Any(a => a.Id == _currentUser.Id) ||
                           e.Invitations.Any(i => i.UserId == _currentUser.Id))
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
        public ICommand PreviousEventCommand { get; }
        public ICommand NextEventCommand { get; }
    }
}