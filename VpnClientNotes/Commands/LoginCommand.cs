using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для авторизации пользователя в системе.
    /// </summary>
    public class LoginCommand : ICommand
    {
        public string Name => "--login";
        public string Description => "Авторизация в системе. Формат: --login \"Логин\" \"Пароль\"";

        // Команда доступна всем (даже неавторизованным)
        public Role[] AllowedRoles => null;

        public void Execute(string[] args)
        {
            // Проверяем количество аргументов
            if (args.Length != 2)
            {
                throw new CommandSyntaxException("Неверное количество аргументов. Ожидается: --login \"Логин\" \"Пароль\"");
            }

            string login = args[0];
            string password = args[1];

            // Открываем подключение к БД
            using (var db = new AppDbContext())
            {
                // Ищем пользователя в базе
                // Хэшируем введенный пользователем пароль и сравниваем с хэшем в базе
                string hashedInputPassword = HashHelper.ComputeSha256Hash(password);
                var user = db.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == hashedInputPassword);

                if (user == null)
                {
                    // Бросаем нашу кастомную ошибку (она автоматически запишется в лог)
                    throw new UnauthorizedException($"Неверный логин или пароль для пользователя '{login}'.");
                }

                // Если нашли - сохраняем сессию локально
                SessionManager.SaveSession(user);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Успешная авторизация! Добро пожаловать, {user.Login} (Роль: {user.Role}).");
                Console.ResetColor();
            }
        }
    }
}
