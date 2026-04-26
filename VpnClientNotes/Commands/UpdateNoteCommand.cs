using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для обновления текста существующей заметки.
    /// </summary>
    public class UpdateNoteCommand : ICommand
    {
        public string Name => "--updatenote";
        public string Description => "Обновить текст заметки. Формат: --updateNote \"ID заметки\" \"Новый текст\"";

        public Role[] AllowedRoles => new[] { Role.User, Role.Admin };

        public void Execute(string[] args)
        {
            if (args.Length != 2)
            {
                throw new CommandSyntaxException("Неверное количество аргументов. Ожидается: --updateNote \"ID\" \"Новый текст\"");
            }

            if (!int.TryParse(args[0], out int noteId))
            {
                throw new CommandSyntaxException("ID заметки должен быть числом.");
            }

            string newText = args[1];
            int currentUserId = SessionManager.GetCurrentUserId().Value;
            Role currentUserRole = SessionManager.GetCurrentUserRole();

            using (var db = new AppDbContext())
            {
                var note = db.Notes.FirstOrDefault(n => n.Id == noteId);

                if (note == null)
                {
                    throw new NotFoundException($"Заметка с ID {noteId} не найдена."); // Используем базовое исключение для вывода
                }

                // Проверка прав: Пользователь может менять только свои заметки, Админ - любые.
                if (note.UserId != currentUserId && currentUserRole != Role.Admin)
                {
                    throw new UnauthorizedException("У вас нет прав на редактирование чужой заметки.");
                }

                // Обновляем текст
                note.Text = newText;
                db.SaveChanges(); // EF Core сам поймет, что нужно сделать UPDATE в базе

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Заметка [ID: {noteId}] успешно обновлена!");
                Console.ResetColor();
            }
        }
    }
}
