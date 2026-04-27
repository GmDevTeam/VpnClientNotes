using VpnClientNotes.Services;
using VpnClientNotes.Utils;
using System;
using VpnClientNotes.Commands;
using System.Threading.Tasks;

namespace VpnClientNotes
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            LoggerService.LogInfo("Запуск приложения...");

            // Проверяем актуальность сессии (правило 30 минут)
            SessionManager.ValidateSessionOnStartup();

            // Запускаем проверку обновлений с GitHub
            await UpdateService.CheckForUpdatesAsync();

            // Запускаем фоновый сервис аналитики
            WatchDogService.Start();

            // Инициализация (регистрация) команд
            CommandProcessor.RegisterCommand(new HelpCommand());
            // Здесь будем добавлять: CommandProcessor.RegisterCommand(new LoginCommand()); и т.д.

            Console.WriteLine("Добро пожаловать в консольный клиент заметок VPN Сервиса!");
            Console.WriteLine("Введите --help для получения Markdown инструкции.");

            // Инициализация (регистрация) команд
            CommandProcessor.RegisterCommand(new HelpCommand());
            CommandProcessor.RegisterCommand(new LoginCommand());
            CommandProcessor.RegisterCommand(new AddNoteCommand());
            CommandProcessor.RegisterCommand(new ShowNotesCommand());
            CommandProcessor.RegisterCommand(new UpdateNoteCommand());
            CommandProcessor.RegisterCommand(new DeleteNoteCommand());
            CommandProcessor.RegisterCommand(new ShowStatsCommand());
            CommandProcessor.RegisterCommand(new ConfigWatchDogCommand());
            CommandProcessor.RegisterCommand(new RegisterCommand());
            CommandProcessor.RegisterCommand(new LogoutCommand());
            CommandProcessor.RegisterCommand(new FreezeUserCommand());
            CommandProcessor.RegisterCommand(new ChangeRoleCommand());


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
