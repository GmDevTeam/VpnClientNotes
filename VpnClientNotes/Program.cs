using VpnClientNotes.Services;
using VpnClientNotes.Utils;
using System;
using VpnClientNotes.Commands;

namespace VpnClientNotes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggerService.LogInfo("Запуск приложения...");

            // Инициализация (регистрация) команд
            CommandProcessor.RegisterCommand(new HelpCommand());
            // Здесь будем добавлять: CommandProcessor.RegisterCommand(new LoginCommand()); и т.д.

            Console.WriteLine("Добро пожаловать в консольный клиент заметок VPN Сервиса!");
            Console.WriteLine("Введите --help для получения Markdown инструкции.");

            // Основной цикл программы
            while (true)
            {
                Console.Write("\n" + GetPrompt());
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.Trim().ToLower() == "--exit")
                {
                    LoggerService.LogInfo("Завершение работы пользователем.");
                    break;
                }

                // Отправляем строку в наш мощный обработчик команд
                CommandProcessor.ProcessInput(input);
            }
        }

        private static string GetPrompt()
        {
            if (SessionManager.IsAuthenticated())
            {
                var role = SessionManager.GetCurrentUserRole();
                return $"VPN-{role}> "; // Будет писать VPN-Admin> или VPN-User>
            }

            return "Guest> ";
        }
    }
}
