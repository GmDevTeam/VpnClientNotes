using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для вывода списка всех заметок текущего пользователя.
    /// </summary>
    public class ShowNotesCommand : ICommand
    {
        public string Name => "--shownotes";
        public string Description => "Показать все мои заметки. Формат: --showNotes";

        // Доступно пользователям и админам
        public Role[] AllowedRoles => new[] { Role.User, Role.Admin };

        public void Execute(string[] args)
        {
            int? currentUserId = SessionManager.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedException("Критическая ошибка сессии: ID пользователя не найден.");
            }

            using (var db = new AppDbContext())
            {
                // Ищем в БД заметки, где UserId совпадает с ID текущего пользователя
                // OrderByDescending сортирует от новых к старым
                var userNotes = db.Notes
                                  .Where(n => n.UserId == currentUserId.Value)
                                  .OrderByDescending(n => n.CreatedAt)
                                  .ToList();

                if (userNotes.Count == 0)
                {
                    Console.WriteLine("У вас пока нет сохраненных заметок.");
                    return;
                }

                Console.WriteLine($"\n--- Ваши заметки (Всего: {userNotes.Count}) ---");
                foreach (var note in userNotes)
                {
                    // Выводим ID заметки, дату и сам текст
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"[ID: {note.Id} | {note.CreatedAt:dd.MM.yyyy HH:mm}] ");
                    Console.ResetColor();
                    Console.WriteLine(note.Text);
                }
                Console.WriteLine("----------------------------------\n");
            }
        }
    }
}
