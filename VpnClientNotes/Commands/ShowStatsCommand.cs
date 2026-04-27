using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для просмотра собранной статистики WatchDog.
    /// Доступна Администратору и Аналитику.
    /// </summary>
    public class ShowStatsCommand : ICommand
    {
        public string Name => "--showstats";
        public string Description => "Показать последние данные WatchDog. Формат: --showStats";

        // Доступно только Админу и Аналитику
        public Role[] AllowedRoles => new[] { Role.Analyst };

        public void Execute(string[] args)
        {
            using (var db = new AppDbContext())
            {
                // Берем 10 самых свежих записей из БД
                var stats = db.SystemStats
                              .OrderByDescending(s => s.RecordedAt)
                              .Take(10)
                              .ToList();

                if (stats.Count == 0)
                {
                    Console.WriteLine("Нет собранных данных.");
                    return;
                }

                Console.WriteLine("\n--- Последние 10 записей статистики системы ---");
                foreach (var s in stats)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"[{s.RecordedAt:HH:mm:ss}] ");
                    Console.ResetColor();

                    // Форматированный вывод
                    Console.WriteLine($"CPU: {s.CpuUsagePercent}% | RAM (свободно): {s.RamAvailableMB} MB | HDD (свободно): {s.HddAvailableGB} GB");
                }
                Console.WriteLine("-----------------------------------------------\n");
            }
        }
    }
}
