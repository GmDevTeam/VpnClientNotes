using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class ChangeRoleCommand : ICommand
    {
        public string Name => "--changerole";
        public string Description => "Изменить роль пользователя. Формат: --changeRole \"Логин\" \"User/Admin/Analyst\"";

        // ТОЛЬКО ДЛЯ АДМИНОВ
        public Role[] AllowedRoles => new[] { Role.Admin };

        public void Execute(string[] args)
        {
            if (args.Length != 2) throw new CommandSyntaxException("Ожидается 2 аргумента: \"Логин\" \"НоваяРоль\"");

            string login = args[0];
            string newRoleString = args[1];

            // Проверяем, существует ли такая роль в нашем enum Role
            if (!Enum.TryParse<Role>(newRoleString, true, out Role newRole))
            {
                throw new CommandSyntaxException($"Роль '{newRoleString}' не найдена. Доступно: User, Admin, Analyst.");
            }

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user == null) throw new NotFoundException($"Пользователь '{login}' не найден.");

                user.Role = newRole;
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Пользователю '{login}' успешно назначена роль: {newRole}.");
                Console.ResetColor();
            }
        }
    }
}
