using System;
using System.Collections.Generic;
using System.Text;

namespace VpnClientNotes.Models
{
    /// <summary>
    /// Сущность пользователя системы.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; } // Храним хэш, а не голый пароль
        public Role Role { get; set; }

        // Навигационное свойство для Entity Framework
        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
