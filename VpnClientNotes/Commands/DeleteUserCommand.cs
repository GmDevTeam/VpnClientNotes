using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class DeleteUserCommand : ICommand
    {
        public string Name => "--deleteuser";
        public string Description => "Удалить профиль пользователя. Формат: --deleteUser \"Логин\"";
        public Role[] AllowedRoles => new[] { Role.Admin };

        public void Execute(string[] args)
        {
            if (args.Length != 1) throw new CommandSyntaxException("Ожидается 1 аргумент: \"Логин\"");
            string login = args[0];

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user == null) throw new NotFoundException($"Пользователь '{login}' не найден.");

                // Защита: нельзя удалить самого себя или главного админа (опционально, но полезно)
                if (user.Login == "admin") throw new UnauthorizedException("Невозможно удалить системного администратора.");

                db.Users.Remove(user);
                db.SaveChanges(); // Благодаря настройкам EF Core, все его заметки тоже удалятся автоматически

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Профиль пользователя '{login}' и все его данные безвозвратно удалены.");
                Console.ResetColor();
            }
        }
    }
}
