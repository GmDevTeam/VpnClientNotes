using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using VpnClientNotes.Models;

namespace VpnClientNotes.Utils
{
    /// <summary>
    /// Класс для управления локальной сессией пользователя.
    /// </summary>
    public static class SessionManager
    {
        private static readonly string SessionFile = "session.json";

        public static void SaveSession(User user)
        {
            var sessionData = new { UserId = user.Id, Role = user.Role.ToString() };
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

        public static void Logout()
        {
            if (File.Exists(SessionFile))
            {
                File.Delete(SessionFile);
            }
        }

        public static Role GetCurrentUserRole()
        {
            if (!IsAuthenticated()) return Role.User; // По умолчанию, хотя без авторизации сюда не дойдет

            string json = File.ReadAllText(SessionFile);
            using JsonDocument doc = JsonDocument.Parse(json);
            string roleString = doc.RootElement.GetProperty("Role").GetString();

            return Enum.Parse<Role>(roleString);
        }
    }
}
