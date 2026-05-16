using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class FreezeUserCommand : ICommand
    {
        public string Name => "--freezeuser";
        public string Description => "Заморозить профиль пользователя. Формат: --freezeUser \"Логин\" \"дни.часы.минуты\" (например: 7/0/0 для недели)";

        // ТОЛЬКО ДЛЯ АДМИНОВ
        public Role[] AllowedRoles => new[] { Role.Admin };

        public void Execute(string[] args)
        {
            if (args.Length != 2) throw new CommandSyntaxException("Ожидается 2 аргумента: \"Логин\" \"дни/часы/минуты\"");

            string login = args[0];
            string timeString = args[1];

            // Парсим формат дни.часы.минуты
            string[] timeParts = timeString.Split('/', '.');
            if (timeParts.Length != 3 ||
                !int.TryParse(timeParts[0], out int days) ||
                !int.TryParse(timeParts[1], out int hours) ||
                !int.TryParse(timeParts[2], out int minutes))
            {
                throw new CommandSyntaxException("Неверный формат времени. Используйте: дни/часы/минуты (например: 7/0/0 для недели).");
            }

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user == null) throw new NotFoundException($"Пользователь '{login}' не найден.");

                // Рассчитываем время окончания заморозки
                TimeSpan banDuration = new TimeSpan(days, hours, minutes, 0);
                user.BannedUntil = DateTime.Now.Add(banDuration);
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Пользователь '{login}' успешно заморожен на {days} дн/ {hours} ч/ {minutes} мин.");
                Console.ResetColor();
            }
        }
    }
}
