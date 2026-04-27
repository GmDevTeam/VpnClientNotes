using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    public class RegisterCommand : ICommand
    {
        public string Name => "--register";
        public string Description => "Регистрация нового пользователя. Формат: --register \"Логин\" \"Пароль\"";

        public Role[] AllowedRoles => null;

        public void Execute(string[] args)
        {
            if (args.Length != 2)
            {
                throw new CommandSyntaxException("Неверное количество аргументов. Ожидается: --register \"Логин\" \"Пароль\"");
            }

            string login = args[0];
            string rawPassword = args[1];

            // 1. ПРОВЕРКА СЛОЖНОСТИ ПАРОЛЯ
            if (rawPassword.Length < 6)
            {
                throw new PasswordComplexityException("Пароль должен содержать не менее 6 символов.");
            }
            if (!rawPassword.Any(char.IsUpper))
            {
                throw new PasswordComplexityException("В пароле должна присутствовать хотя бы одна заглавная буква.");
            }
            if (!rawPassword.Any(char.IsDigit))
            {
                throw new PasswordComplexityException("В пароле должна присутствовать хотя бы одна цифра.");
            }
            // Проверяем спецсимвол (символ, который не является ни буквой, ни цифрой)
            if (!rawPassword.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                throw new PasswordComplexityException("В пароле должен присутствовать хотя бы один специальный символ (например: _, !, @, #, $, %, ^, &, *).");
            }

            // 2. РАБОТА С БАЗОЙ ДАННЫХ
            using (var db = new AppDbContext())
            {
                // ИСПРАВЛЕНИЕ ОШИБКИ CS0144: Используем конкретный класс ошибки
                if (db.Users.Any(u => u.Login == login))
                {
                    throw new UserAlreadyExistsException($"Пользователь с логином '{login}' уже существует.");
                }

                var newUser = new User
                {
                    Login = login,
                    PasswordHash = HashHelper.ComputeSha256Hash(rawPassword),
                    Role = Role.User
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Пользователь '{login}' успешно зарегистрирован! Теперь вы можете войти, используя --login.");
                Console.ResetColor();
            }
        }
    }
}
