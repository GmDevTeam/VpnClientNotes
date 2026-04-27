using System;
using System.IO;
using System.Linq;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class ShowLogsCommand : ICommand
    {
        public string Name => "--showlogs";
        public string Description => "Вывод последних 20 строк системного лога. Формат: --showLogs";
        public Role[] AllowedRoles => new[] { Role.Admin };

        public void Execute(string[] args)
        {
            string logFile = "system_logs.txt";

            if (!File.Exists(logFile))
            {
                Console.WriteLine("Файл логов пуст или еще не создан.");
                return;
            }

            try
            {
                // Читаем все строки, берем последние 20 штук
                var lines = File.ReadLines(logFile).Reverse().Take(20).Reverse().ToList();

                Console.WriteLine("\n--- Последние 20 записей system_logs.txt ---");
                foreach (var line in lines)
                {
                    if (line.Contains("[ERROR]")) Console.ForegroundColor = ConsoleColor.Red;
                    else if (line.Contains("[INFO]")) Console.ForegroundColor = ConsoleColor.DarkGray;

                    Console.WriteLine(line);
                    Console.ResetColor();
                }
                Console.WriteLine("----------------------------------------------\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении логов: {ex.Message}");
            }
        }
    }
}
