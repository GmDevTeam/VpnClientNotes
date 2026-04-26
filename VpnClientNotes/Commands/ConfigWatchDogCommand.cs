using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для изменения настроек WatchDog на лету.
    /// </summary>
    public class ConfigWatchDogCommand : ICommand
    {
        public string Name => "--configwatchdog";
        public string Description => "Настройка WatchDog. Формат: --configWatchDog \"Ключ (cpu/ram/hdd/interval/isactive)\" \"Значение (true/false или число)\"";

        public Role[] AllowedRoles => new[] { Role.Admin, Role.Analyst };

        public void Execute(string[] args)
        {
            if (args.Length != 2)
            {
                throw new CommandSyntaxException("Ожидается 2 аргумента: Ключ и Значение.");
            }

            string key = args[0].ToLower();
            string value = args[1].ToLower();

            using (var db = new AppDbContext())
            {
                var settings = db.WatchDogSettings.FirstOrDefault();
                if (settings == null) throw new NotFoundException("Настройки WatchDog не найдены в БД.");

                // Обработка ключей
                switch (key)
                {
                    case "isactive":
                        if (!bool.TryParse(value, out bool activeRes)) throw new CommandSyntaxException("Значение должно быть true или false.");
                        settings.IsActive = activeRes;
                        break;

                    case "cpu":
                        if (!bool.TryParse(value, out bool cpuRes)) throw new CommandSyntaxException("Значение должно быть true или false.");
                        settings.TrackCpu = cpuRes;
                        break;

                    case "ram":
                        if (!bool.TryParse(value, out bool ramRes)) throw new CommandSyntaxException("Значение должно быть true или false.");
                        settings.TrackRam = ramRes;
                        break;

                    case "hdd":
                        if (!bool.TryParse(value, out bool hddRes)) throw new CommandSyntaxException("Значение должно быть true или false.");
                        settings.TrackHdd = hddRes;
                        break;

                    case "interval":
                        if (!int.TryParse(value, out int intervalRes) || intervalRes < 1) throw new CommandSyntaxException("Интервал должен быть целым числом больше 0.");
                        settings.IntervalSeconds = intervalRes;
                        break;

                    default:
                        throw new CommandSyntaxException($"Неизвестный ключ '{key}'. Доступные ключи: isactive, cpu, ram, hdd, interval.");
                }

                db.SaveChanges(); // Сохраняем в БД

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Настройка '{key}' успешно изменена на '{value}'. Изменения применятся со следующего тика таймера.");
                Console.ResetColor();
            }
        }
    }
}
