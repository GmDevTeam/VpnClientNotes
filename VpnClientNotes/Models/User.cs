using System;
using System.Collections.Generic;

namespace VpnClientNotes.Models
{
    /// <summary>
    /// Сущность пользователя системы.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public Role Role { get; set; }

        // Время, до которого профиль заморожен. 
        // Знак вопроса означает, что оно может быть null (профиль не заморожен)
        public DateTime? BannedUntil { get; set; }

        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
