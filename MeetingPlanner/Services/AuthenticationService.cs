using MeetingPlanner.Models;
using System.Linq;
using BCrypt.Net;

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
            if (_db.Users.Any(u => u.Username == username))
            {
                return false; // Пользователь с таким именем уже существует
            }

            // Хешируем пароль с использованием BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User { Username = username, Password = hashedPassword };
            _db.Users.Add(newUser);
            _db.SaveChanges();
            return true;
        }

        public User AuthenticateUser(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return null; // Пользователь не найден
            }

            // Верифицируем введенный пароль с хешем из БД
            bool passwordMatches = BCrypt.Net.BCrypt.Verify(password, user.Password);

            if (passwordMatches)
            {
                return user;
            }
            else
            {
                return null; // Неверный пароль
            }
        }
    }
}