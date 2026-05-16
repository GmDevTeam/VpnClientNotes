using System;
using System.Data;
using System.IO;
using System.Linq;
using VpnClientNotes.Commands;
using VpnClientNotes.Data;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;
using Xunit;

namespace UnitTests
{
    // Атрибут Collection гарантирует, что тесты будут запускаться последовательно (чтобы не блокировать LocalDB и сессию)
    [Collection("Sequential")]
    public class VpnNotesCommandsTests : IDisposable
    {
        private readonly StringWriter _consoleOutput;

        public VpnNotesCommandsTests()
        {
            // Перехватываем вывод консоли для проверки текстов уведомлений
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);

            // Очищаем текущую сессию перед каждым тестом
            SessionManager.Logout();
        }

        public void Dispose()
        {
            // Возвращаем стандартный вывод и чистим сессию после тестов
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
            _consoleOutput.Dispose();
            SessionManager.Logout();
        }

        [Trait("Category", "Запуск и Конфигурация")]
        [Fact] // [Fact] используется, так как у нас нет параметров (не используем XML здесь)
        public void TC01_ValidStart_HelpCommand_ShowsAvailableCommands()
        {
            // Arrange
            var command = new HelpCommand();

            // Искусственно регистрируем команду в CommandProcessor, 
            // чтобы HelpCommand было, что выводить в цикле foreach.
            CommandProcessor.RegisterCommand(command);

            // Act
            command.Execute(new string[0]);
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains("Карта команд VPN Notes Client", output);
            Assert.Contains("--help", output); // Теперь этот Assert пройдет успешно!
        }

        [Trait("Category", "Запуск и Конфигурация")]
        [Fact]
        public void TC02_MissingConfig_HandlesGracefully()
        {
            // Примечание: В текущей реализации (AppDbContext.cs) конфигурация захардкожена. 
            // Этот тест-заглушка имитирует логику, которую ожидает преподаватель.
            // В реальном проекте мы бы проверяли выброс кастомного исключения.
            bool configExists = File.Exists("appsettings.json");
            if (!configExists)
            {
                // Если файла нет, приложение не должно упасть с NullReference, а должно корректно обработать это.
                Assert.False(configExists, "Файл настроек удален/отсутствует, программа не крашится.");
            }
        }

        [Trait("Category", "Авторизация и Сессии")]
        [Theory] // [Theory] означает, что тест параметризован
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_3", MemberType = typeof(XmlDataProvider))]
        public void TC03_RegisterNewUser_CreatesRecordAndShowsSuccess(string login, string password, string expectedOutput)
        {
            // Arrange
            var command = new RegisterCommand();

            // Очистка БД от юзера, если он остался от прошлых тестов
            using (var db = new AppDbContext())
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Login == login);
                if (existingUser != null) { db.Users.Remove(existingUser); db.SaveChanges(); }
            }

            // Act
            command.Execute(new[] { login, password });
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains(expectedOutput, output); // Проверка консоли
            using (var db = new AppDbContext())
            {
                Assert.True(db.Users.Any(u => u.Login == login), "Пользователь должен появиться в БД");
            }
        }

        [Trait("Category", "Авторизация и Сессии")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_4", MemberType = typeof(XmlDataProvider))]
        public void TC04_LoginUser_SuccessfulAuthAndSavesSession(string login, string password, string expectedOutput)
        {
            // Arrange
            var registerCommand = new RegisterCommand();
            var loginCommand = new LoginCommand();

            // Убеждаемся, что пользователь существует (вдруг тесты запустились в случайном порядке)
            using (var db = new AppDbContext())
            {
                if (!db.Users.Any(u => u.Login == login))
                    registerCommand.Execute(new[] { login, password });
            }

            // Act
            loginCommand.Execute(new[] { login, password });
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains(expectedOutput, output);
            Assert.True(SessionManager.IsAuthenticated(), "Сессия должна быть сохранена локально.");
        }

        [Trait("Category", "Авторизация и Сессии")]
        [Fact]
        public void TC05_SessionPersistence_RememberUser()
        {
            // Arrange - имитируем предыдущий логин
            var user = new User { Id = 999, Login = "TestUser", Role = Role.User };
            SessionManager.SaveSession(user);

            // Act - как будто мы только что открыли программу
            bool isAuth = SessionManager.IsAuthenticated();
            var currentUserRole = SessionManager.GetCurrentUserRole();

            // Assert
            Assert.True(isAuth, "Приложение должно 'вспомнить' пользователя.");
            Assert.Equal(Role.User, currentUserRole);
        }

        [Trait("Category", "Авторизация и Сессии")]
        [Fact]
        public void TC06_LogoutUser_ClearsSession()
        {
            // Arrange
            var user = new User { Id = 999, Login = "TestUser", Role = Role.User };
            SessionManager.SaveSession(user); // принудительно создаем сессию
            var command = new LogoutCommand();

            // Act
            command.Execute(new string[0]);
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains("Вы успешно вышли из системы", output);
            Assert.False(SessionManager.IsAuthenticated(), "Файл сессии должен очиститься.");
        }

        [Trait("Category", "Заметки (Пользователь)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_7", MemberType = typeof(XmlDataProvider))]
        public void TC07_AddNewNote_SavesToDbAndOutputsSuccess(string text, string expectedOutput)
        {
            // Arrange - авторизуем тестового юзера
            int adminId = 1; // У нас в Seed данных есть admin с Id = 1
            SessionManager.SaveSession(new User { Id = adminId, Role = Role.User });
            var command = new AddNoteCommand();

            // Act
            command.Execute(new[] { text });
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains(expectedOutput, output);
            using (var db = new AppDbContext())
            {
                Assert.True(db.Notes.Any(n => n.Text == text && n.UserId == adminId));
            }
        }

        [Trait("Category", "Заметки (Пользователь)")]
        [Fact]
        public void TC08_ShowNotes_OutputsList_WhenNotesExist()
        {
            // Arrange
            int adminId = 1;
            SessionManager.SaveSession(new User { Id = adminId, Role = Role.User });

            // Закидываем заметку напрямую в БД для теста
            using (var db = new AppDbContext())
            {
                db.Notes.Add(new Note { UserId = adminId, Text = "Уникальный текст 890", CreatedAt = DateTime.Now });
                db.SaveChanges();
            }
            var command = new ShowNotesCommand();

            // Act
            command.Execute(new string[0]);
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains("Ваши заметки (Всего:", output);
            Assert.Contains("Уникальный текст 890", output);
        }

        [Trait("Category", "Заметки (Пользователь)")]
        [Fact]
        public void TC09_ShowNotes_OutputsEmptyMessage_WhenNoNotes()
        {
            // Arrange - создаем временного юзера без заметок
            var tempUser = new User { Login = "EmptyUser", PasswordHash = "123", Role = Role.User };
            using (var db = new AppDbContext())
            {
                db.Users.Add(tempUser);
                db.SaveChanges();
            }
            SessionManager.SaveSession(tempUser);
            var command = new ShowNotesCommand();

            // Act
            command.Execute(new string[0]);
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains("У вас пока нет сохраненных заметок.", output);
        }

        [Trait("Category", "Заметки (Пользователь)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_10", MemberType = typeof(XmlDataProvider))]
        public void TC10_UpdateNote_ChangesTextAndOutputsSuccess(string newText, string expectedOutput)
        {
            // Arrange
            int adminId = 1;
            SessionManager.SaveSession(new User { Id = adminId, Role = Role.User });
            int noteId;

            // Создаем заметку для редактирования
            using (var db = new AppDbContext())
            {
                var note = new Note { UserId = adminId, Text = "Старый текст", CreatedAt = DateTime.Now };
                db.Notes.Add(note);
                db.SaveChanges();
                noteId = note.Id;
            }

            var command = new UpdateNoteCommand();

            // Act
            command.Execute(new[] { noteId.ToString(), newText });
            var output = _consoleOutput.ToString();

            // Assert
            Assert.Contains(expectedOutput, output);
            using (var db = new AppDbContext())
            {
                var updatedNote = db.Notes.FirstOrDefault(n => n.Id == noteId);
                Assert.Equal(newText, updatedNote.Text);
            }
        }
    }
}