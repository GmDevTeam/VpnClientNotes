using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Services;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Ядро обработки команд. Регистрирует команды и управляет их вызовом.
    /// </summary>
    public static class CommandProcessor
    {
        // Словарь всех команд в системе
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        /// <summary>
        /// Регистрация новой команды в системе.
        /// </summary>
        public static void RegisterCommand(ICommand command)
        {
            _commands[command.Name.ToLower()] = command;
        }

        /// <summary>
        /// Возвращает список всех зарегистрированных команд (для меню Help).
        /// </summary>
        public static IEnumerable<ICommand> GetAllCommands() => _commands.Values;

        /// <summary>
        /// Главный метод обработки ввода пользователя.
        /// </summary>
        public static void ProcessInput(string input)
        {
            try
            {
                // Разбиваем строку на аргументы, учитывая текст в кавычках как один аргумент
                // Например: --addNewNote "Hello World" -> ["--addNewNote", "Hello World"]
                var args = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                                .Select(m => m.Value.Trim('"'))
                                .ToArray();

                if (args.Length == 0) return;

                string commandName = args[0].ToLower();
                string[] commandArgs = args.Skip(1).ToArray();

                // 1. Проверка существования команды
                if (!_commands.ContainsKey(commandName))
                {
                    throw new CommandSyntaxException($"Команда '{commandName}' не найдена. Введите --help для списка команд.");
                }

                ICommand command = _commands[commandName];

                // 2. Проверка прав доступа (Авторизация и Роли)
                if (command.AllowedRoles != null)
                {
                    if (!SessionManager.IsAuthenticated())
                    {
                        throw new UnauthorizedException($"Для выполнения команды '{commandName}' необходимо авторизоваться (используйте --login).");
                    }

                    Role currentUserRole = SessionManager.GetCurrentUserRole();
                    if (!command.AllowedRoles.Contains(currentUserRole))
                    {
                        throw new UnauthorizedException($"У вас (Роль: {currentUserRole}) нет прав для выполнения команды '{commandName}'.");
                    }
                }

                // 3. Выполнение команды
                command.Execute(commandArgs);
            }
            catch (AppBaseException ex) // Ловим наши кастомные ошибки (они уже записались в лог)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex) // Ловим системные ошибки
            {
                LoggerService.LogError($"Критическая ошибка при выполнении: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Произошла системная ошибка. Логи сохранены.");
                Console.ResetColor();
            }
        }
    }
}
