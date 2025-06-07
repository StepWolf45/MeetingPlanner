using MeetingPlanner.Models;
using System.Linq;

namespace MeetingPlanner.Services
{
    public class AuthenticationService
    {
        private readonly DatabaseService _db;

        public AuthenticationService(DatabaseService db)
        {
            _db = db;
        }

        public bool RegisterUser(string username, string password)
        {
            // Быстрая проверка существования пользователя перед тяжелыми операциями
            if (_db.Users.AsNoTracking().Any(u => u.Username == username))
            {
                return false;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var newUser = new User { Username = username, Password = hashedPassword };

            _db.Users.Add(newUser);
            _db.SaveChanges();
            return true;
        }

        public User AuthenticateUser(string username, string password)
        {
            // Используем AsNoTracking для ускорения, т.к. нам не нужно отслеживать изменения
            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                // Возвращаем null сразу, без вычисления хеша
                return null;
            }

            // Добавляем быструю проверку длины хеша перед сложной проверкой
            if (user.Password?.Length != 60) // BCrypt hash всегда 60 символов
            {
                return null;
            }

            bool passwordMatches = BCrypt.Net.BCrypt.Verify(password, user.Password);

            return passwordMatches ? user : null;
        }
    }
}