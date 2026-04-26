using VpnClientNotes.Services;
using VpnClientNotes.Utils;

namespace VpnClientNotes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggerService.LogInfo("Запуск приложения...");

            // Имитация проверки обновлений
            CheckForUpdates();

            Console.WriteLine("\nДобро пожаловать в консольный клиент заметок VPN Сервиса!");
            Console.WriteLine("Введите --help для получения инструкции.");

            // Основной цикл программы
            while (true)
            {
                Console.Write("\n" + GetPrompt());
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.Trim().ToLower() == "exit")
                {
                    LoggerService.LogInfo("Завершение работы...");
                    break;
                }

                // Здесь позже будет вызов CommandHandler
                // CommandHandler.Execute(input);
            }
        }

        /// <summary>
        /// Формирует строку приглашения для консоли в зависимости от авторизации.
        /// </summary>
        private static string GetPrompt()
        {
            if (SessionManager.IsAuthenticated())
                return "VPN-User> ";

            return "Guest> ";
        }

        /// <summary>
        /// Имитация проверки обновлений системы при старте.
        /// </summary>
        private static void CheckForUpdates()
        {
            Console.WriteLine("Проверка обновлений...");
            // Имитация: с вероятностью 30% есть обновление
            if (new Random().Next(0, 100) > 70)
            {
                Console.Write("Доступно обновление. Обновить? (Да/Нет): ");
                string answer = Console.ReadLine()?.ToLower();
                if (answer == "да" || answer == "yes" || answer == "y")
                {
                    Console.WriteLine("Обновление скачано и установлено. Перезапуск...");
                    LoggerService.LogInfo("Система обновлена.");
                }
            }
        }
    }
}
