using System;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для выхода из учетной записи (удаление сессии).
    /// </summary>
    public class LogoutCommand : ICommand
    {
        public string Name => "--logout";
        public string Description => "Выйти из текущей учетной записи. Формат: --logout";

        // Команда доступна только тем, кто уже вошел (Авторизован)
        public Role[] AllowedRoles => new[] { Role.User, Role.Admin, Role.Analyst };

        public void Execute(string[] args)
        {
            SessionManager.Logout();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Вы успешно вышли из системы. Теперь вы Гость.");
            Console.ResetColor();
        }
    }
}
