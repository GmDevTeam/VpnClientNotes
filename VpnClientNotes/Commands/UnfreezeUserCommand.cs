using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class UnfreezeUserCommand : ICommand
    {
        public string Name => "--unfreezeuser";
        public string Description => "Досрочно разморозить профиль. Формат: --unfreezeUser \"Логин\"";

        public Role[] AllowedRoles => new[] { Role.Admin };

        public void Execute(string[] args)
        {
            if (args.Length != 1) throw new CommandSyntaxException("Ожидается 1 аргумент: \"Логин\"");

            string login = args[0];

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user == null) throw new NotFoundException($"Пользователь '{login}' не найден.");

                if (!user.BannedUntil.HasValue)
                {
                    Console.WriteLine($"Профиль '{login}' и так не заморожен.");
                    return;
                }

                // Снимаем блокировку, устанавливая null
                user.BannedUntil = null;
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Пользователь '{login}' досрочно разморожен!");
                Console.ResetColor();
            }
        }
    }
}
