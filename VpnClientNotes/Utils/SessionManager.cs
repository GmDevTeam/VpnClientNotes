using System;
using System.IO;
using System.Text.Json;
using VpnClientNotes.Models;
using VpnClientNotes.Services;

namespace VpnClientNotes.Utils
{
    public static class SessionManager
    {
        private static readonly string SessionFile = "session.json";

        public static void SaveSession(User user)
        {
            // Теперь сохраняем и время создания сессии!
            var sessionData = new
            {
                UserId = user.Id,
                Role = user.Role.ToString(),
                CreatedAt = DateTime.Now // <-- Добавлено
            };
            string json = JsonSerializer.Serialize(sessionData);
            File.WriteAllText(SessionFile, json);
        }

        public static bool IsAuthenticated()
        {
            return File.Exists(SessionFile);
        }

        public static int? GetCurrentUserId()
        {
            if (!IsAuthenticated()) return null;
            string json = File.ReadAllText(SessionFile);
            using JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("UserId").GetInt32();
        }

        public static Role GetCurrentUserRole()
        {
            if (!IsAuthenticated()) return Role.User;
            string json = File.ReadAllText(SessionFile);
            using JsonDocument doc = JsonDocument.Parse(json);
            string roleString = doc.RootElement.GetProperty("Role").GetString();
            return Enum.Parse<Role>(roleString);
        }

        public static void Logout()
        {
            if (File.Exists(SessionFile))
            {
                File.Delete(SessionFile);
            }
        }

        /// <summary>
        /// Проверяет, не устарела ли сессия (более 30 минут). 
        /// Вызывается только 1 раз при запуске программы.
        /// </summary>
        public static void ValidateSessionOnStartup()
        {
            if (!IsAuthenticated()) return;

            try
            {
                string json = File.ReadAllText(SessionFile);
                using JsonDocument doc = JsonDocument.Parse(json);
                DateTime createdAt = doc.RootElement.GetProperty("CreatedAt").GetDateTime();

                // Если с момента логина прошло более 30 минут
                if (DateTime.Now - createdAt > TimeSpan.FromMinutes(30))
                {
                    Logout(); // Удаляем файл
                    LoggerService.LogInfo("Сессия пользователя истекла (прошло > 30 мин). Сброс авторизации.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Система] Ваша сессия истекла. Пожалуйста, выполните вход заново (--login).");
                    Console.ResetColor();
                }
            }
            catch
            {
                // Если файл старой версии (без CreatedAt) или поврежден - сбрасываем
                Logout();
            }
        }
    }
}
