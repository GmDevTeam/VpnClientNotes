using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class ShowUsersCommand : ICommand
    {
        public string Name => "--showusers";
        public string Description => "Показать список всех пользователей системы. Формат: --showUsers";
        public Role[] AllowedRoles => new[] { Role.Admin };

        public void Execute(string[] args)
        {
            using (var db = new AppDbContext())
            {
                var users = db.Users.ToList();

                Console.WriteLine("\n--- Список пользователей системы ---");
                foreach (var u in users)
                {
                    string status = u.BannedUntil.HasValue && u.BannedUntil > DateTime.Now
                        ? $"[ЗАМОРОЖЕН до {u.BannedUntil:dd.MM.yyyy HH:mm}]"
                        : "[Активен]";

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"[ID: {u.Id}] ");
                    Console.ResetColor();
                    Console.WriteLine($"Логин: {u.Login} | Роль: {u.Role} {status}");
                }
                Console.WriteLine("------------------------------------\n");
            }
        }
    }
}
