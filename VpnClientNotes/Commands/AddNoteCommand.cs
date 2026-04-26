using System;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Команда для добавления новой текстовой заметки.
    /// </summary>
    public class AddNoteCommand : ICommand
    {
        public string Name => "--addnewnote";
        public string Description => "Создать новую заметку. Формат: --addNewNote \"Текст заметки\"";

        // Разрешаем только Юзерам и Админам
        public Role[] AllowedRoles => new[] { Role.User, Role.Admin };

        public void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                throw new CommandSyntaxException("Неверное количество аргументов. Ожидается: --addNewNote \"Текст заметки\"");
            }

            string text = args[0];

            // Получаем ID текущего авторизованного пользователя
            int? currentUserId = SessionManager.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedException("Критическая ошибка сессии: ID пользователя не найден.");
            }

            // Сохраняем в базу данных
            using (var db = new AppDbContext())
            {
                var note = new Note
                {
                    Text = text,
                    CreatedAt = DateTime.Now,
                    UserId = currentUserId.Value
                };

                db.Notes.Add(note);
                db.SaveChanges(); // Фиксируем изменения в SQL Server
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Заметка успешно добавлена в базу данных!");
            Console.ResetColor();
        }
    }
}
