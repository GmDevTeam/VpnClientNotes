using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для удаления заметки по её ID.
    /// </summary>
    public class DeleteNoteCommand : ICommand
    {
        public string Name => "--deletenote";
        public string Description => "Удалить заметку. Формат: --deleteNote \"ID заметки\"";

        public Role[] AllowedRoles => new[] { Role.User };

        public void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                throw new CommandSyntaxException("Неверное количество аргументов. Ожидается: --deleteNote \"ID\"");
            }

            if (!int.TryParse(args[0], out int noteId))
            {
                throw new CommandSyntaxException("ID заметки должен быть числом.");
            }

            int currentUserId = SessionManager.GetCurrentUserId().Value;
            Role currentUserRole = SessionManager.GetCurrentUserRole();

            using (var db = new AppDbContext())
            {
                var note = db.Notes.FirstOrDefault(n => n.Id == noteId);

                if (note == null)
                {
                    throw new NotFoundException($"Заметка с ID {noteId} не найдена.");
                }

                // Проверка прав на удаление
                if (note.UserId != currentUserId && currentUserRole != Role.Admin)
                {
                    throw new UnauthorizedException("У вас нет прав на удаление чужой заметки.");
                }

                db.Notes.Remove(note);
                db.SaveChanges(); // EF Core выполнит DELETE запрос

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Заметка [ID: {noteId}] безвозвратно удалена.");
                Console.ResetColor();
            }
        }
    }
}
