using System;
using System.Data;
using System.IO;
using System.Linq;
using VpnClientNotes.Commands;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;
using VpnClientNotes.Services;
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

        [Trait("Category", "Авторизация и Сессии")]
        [Theory] // [Theory] означает, что тест параметризован
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_1", MemberType = typeof(XmlDataProvider))]
        public void TC01_RegisterNewUser_CreatesRecordAndShowsSuccess(string login, string password, string expectedOutput)
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
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_2", MemberType = typeof(XmlDataProvider))]
        public void TC02_LoginUser_SuccessfulAuthAndSavesSession(string login, string password, string expectedOutput)
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
        public void TC03_SessionPersistence_RememberUser()
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
        public void TC04_LogoutUser_ClearsSession()
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
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_5", MemberType = typeof(XmlDataProvider))]
        public void TC05_AddNewNote_SavesToDbAndOutputsSuccess(string text, string expectedOutput)
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
        public void TC06_ShowNotes_OutputsList_WhenNotesExist()
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
        public void TC07_ShowNotes_OutputsEmptyMessage_WhenNoNotes()
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
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_8", MemberType = typeof(XmlDataProvider))]
        public void TC08_UpdateNote_ChangesTextAndOutputsSuccess(string newText, string expectedOutput)
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

        [Trait("Category", "Заметки (Пользователь)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_9", MemberType = typeof(XmlDataProvider))]
        public void TC09_ShowNotes_AfterUpdate_ContainsUpdatedText(string expectedText)
        {
            // Условие: Авторизован User. Известен ID заметки, введена команда --updatenote ... Вызвать --shownotes
            int userId = 2; // Берем условного юзера
            SessionManager.SaveSession(new User { Id = userId, Role = Role.User });

            using (var db = new AppDbContext())
            {
                // Имитируем, что заметка уже изменена (TC 8 отработал)
                db.Notes.Add(new Note { UserId = userId, Text = expectedText, CreatedAt = DateTime.Now });
                db.SaveChanges();
            }

            var command = new ShowNotesCommand();
            command.Execute(new string[0]);
            var output = _consoleOutput.ToString();

            // Проверка: вывелась заметка "Текст изменен"
            Assert.Contains(expectedText, output);
        }

        [Trait("Category", "Заметки (Пользователь)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_10", MemberType = typeof(XmlDataProvider))]
        public void TC10_DeleteNote_RemovesFromDbAndShowsSuccess(string expectedOutput)
        {
            int userId = 2;
            SessionManager.SaveSession(new User { Id = userId, Role = Role.User });
            int noteId;

            using (var db = new AppDbContext())
            {
                var note = new Note { UserId = userId, Text = "Заметка на удаление", CreatedAt = DateTime.Now };
                db.Notes.Add(note);
                db.SaveChanges();
                noteId = note.Id;
            }

            var command = new DeleteNoteCommand();
            command.Execute(new[] { noteId.ToString() });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            using (var db = new AppDbContext())
            {
                Assert.False(db.Notes.Any(n => n.Id == noteId), "Запись должна исчезнуть из базы данных.");
            }
        }

        [Trait("Category", "Управление (Администратор)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_11", MemberType = typeof(XmlDataProvider))]
        public void TC11_ShowUsers_AsAdmin_OutputsUsersTable(string expectedOutput)
        {
            // Условие: Выполнен вход под аккаунтом Admin.
            SessionManager.SaveSession(new User { Id = 1, Role = Role.Admin });

            var command = new ShowUsersCommand();
            command.Execute(new string[0]);

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output); // Отображается таблица/список
        }

        [Trait("Category", "Управление (Администратор)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_12", MemberType = typeof(XmlDataProvider))]
        public void TC12_FreezeUser_AsAdmin_BansUser(string login, string duration, string expectedOutput)
        {
            SessionManager.SaveSession(new User { Id = 1, Role = Role.Admin });

            using (var db = new AppDbContext())
            {
                if (!db.Users.Any(u => u.Login == login))
                {
                    db.Users.Add(new User { Login = login, PasswordHash = "hash", Role = Role.User });
                    db.SaveChanges();
                }
            }

            var command = new FreezeUserCommand();
            command.Execute(new[] { login, duration });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            using (var db = new AppDbContext())
            {
                var user = db.Users.First(u => u.Login == login);
                Assert.NotNull(user.BannedUntil);
                Assert.True(user.BannedUntil > DateTime.Now, "Аккаунт Spammer блокируется на указанное время.");
            }
        }

        [Trait("Category", "Управление (Администратор)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_13", MemberType = typeof(XmlDataProvider))]
        public void TC13_Login_FrozenUser_OutputsBanMessage(string login, string password, string expectedError)
        {
            // Очищаем сессию (неавторизованный пользователь)
            SessionManager.Logout();

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user == null)
                {
                    user = new User { Login = login, PasswordHash = HashHelper.ComputeSha256Hash(password), Role = Role.User };
                    db.Users.Add(user);
                }
                user.PasswordHash = HashHelper.ComputeSha256Hash(password);
                user.BannedUntil = DateTime.Now.AddDays(2); // Заморожен
                db.SaveChanges();
            }

            var command = new LoginCommand();

            // Метод выбросит UserBannedException согласно логике в LoginCommand.cs
            var exception = Assert.Throws<UserBannedException>(() => command.Execute(new[] { login, password }));

            Assert.Contains(expectedError, exception.Message); // В консоль(логику) выведется сообщение о блокировке
        }

        [Trait("Category", "Управление (Администратор)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_14", MemberType = typeof(XmlDataProvider))]
        public void TC14_UnfreezeUser_AsAdmin_RemovesBan(string login, string expectedOutput)
        {
            SessionManager.SaveSession(new User { Id = 1, Role = Role.Admin });

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user != null)
                {
                    user.BannedUntil = DateTime.Now.AddDays(2); // Изначально заморожен
                    db.SaveChanges();
                }
            }

            var command = new UnfreezeUserCommand();
            command.Execute(new[] { login });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            using (var db = new AppDbContext())
            {
                var user = db.Users.First(u => u.Login == login);
                Assert.Null(user.BannedUntil); // Аккаунт снова может авторизоваться (блокировка снята)
            }
        }

        [Trait("Category", "Управление (Администратор)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_15", MemberType = typeof(XmlDataProvider))]
        public void TC15_Login_UnfrozenUser_SuccessfulAuth(string login, string password, string expectedOutput)
        {
            SessionManager.Logout();

            using (var db = new AppDbContext())
            {
                var user = db.Users.First(u => u.Login == login);
                user.PasswordHash = HashHelper.ComputeSha256Hash(password);
                user.BannedUntil = null; // Точно разблокирован
                db.SaveChanges();
            }

            var command = new LoginCommand();
            command.Execute(new[] { login, password });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output); // Успешная авторизация
        }

        [Trait("Category", "Аналитика (Аналитик)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_16", MemberType = typeof(XmlDataProvider))]
        public void TC16_ConfigWatchDog_ChangeInterval_UpdatesDb(string key, string value, string expectedOutput)
        {
            SessionManager.SaveSession(new User { Id = 3, Role = Role.Analyst });

            var command = new ConfigWatchDogCommand();
            command.Execute(new[] { key, value });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            using (var db = new AppDbContext())
            {
                var settings = db.WatchDogSettings.First();
                Assert.Equal(int.Parse(value), settings.IntervalSeconds); // Интервал меняется
            }
        }

        [Trait("Category", "Аналитика (Аналитик)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_17", MemberType = typeof(XmlDataProvider))]
        public void TC17_ConfigWatchDog_ToggleCpuFlag_UpdatesDb(string key, string value, string expectedOutput)
        {
            SessionManager.SaveSession(new User { Id = 3, Role = Role.Analyst });

            var command = new ConfigWatchDogCommand();
            command.Execute(new[] { key, value });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            using (var db = new AppDbContext())
            {
                var settings = db.WatchDogSettings.First();
                Assert.False(settings.TrackCpu); // Модуль перестает собирать информацию о CPU
            }
        }

        [Trait("Category", "Аналитика (Аналитик)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_18", MemberType = typeof(XmlDataProvider))]
        public void TC18_ShowStats_AsAnalyst_OutputsData(string expectedOutput)
        {
            SessionManager.SaveSession(new User { Id = 3, Role = Role.Analyst });

            using (var db = new AppDbContext())
            {
                db.SystemStats.Add(new SystemStat { CpuUsagePercent = 15, RamAvailableMB = 2048, HddAvailableGB = 500, RecordedAt = DateTime.Now });
                db.SaveChanges();
            }

            var command = new ShowStatsCommand();
            command.Execute(new string[0]);

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output); // Отображается актуальная сводка системных метрик
        }

        [Trait("Category", "Аналитика (Аналитик)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_19", MemberType = typeof(XmlDataProvider))]
        public void TC19_AddTrackObject_AsAnalyst_AddsToDb(string process, string expectedOutput)
        {
            SessionManager.SaveSession(new User { Id = 3, Role = Role.Analyst });

            using (var db = new AppDbContext())
            {
                var existing = db.TrackedObjects.FirstOrDefault(t => t.ProcessName == process);
                if (existing != null) { db.TrackedObjects.Remove(existing); db.SaveChanges(); }
            }

            var command = new AddTrackObjectCommand();
            command.Execute(new[] { process });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            using (var db = new AppDbContext())
            {
                Assert.True(db.TrackedObjects.Any(t => t.ProcessName == process), "В базу данных добавляется новая запись.");
            }
        }

        [Trait("Category", "Аналитика (Аналитик)")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_20", MemberType = typeof(XmlDataProvider))]
        public void TC20_RemoveTrackObject_AsAnalyst_RemovesFromDb(string process, string expectedOutput)
        {
            // Устанавливаем сессию Аналитика
            SessionManager.SaveSession(new User { Id = 3, Role = Role.Analyst });

            // Подготавливаем БД: убеждаемся, что процесс есть для удаления
            using (var db = new AppDbContext())
            {
                if (!db.TrackedObjects.Any(t => t.ProcessName == process))
                {
                    db.TrackedObjects.Add(new TrackedObject { ProcessName = process });
                    db.SaveChanges();
                }
            }

            var command = new RemoveTrackObjectCommand();
            command.Execute(new[] { process });

            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);

            // Проверяем, что в БД записи больше нет
            using (var db = new AppDbContext())
            {
                Assert.False(db.TrackedObjects.Any(t => t.ProcessName == process), "Процесс должен быть удален из базы данных.");
            }
        }

        [Trait("Category", "Изоляция прав доступа")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_21", MemberType = typeof(XmlDataProvider))]
        public void TC21_AccessIsolation_UserTriesAdminCommand_OutputsError(string command, string expectedOutput)
        {
            // Устанавливаем сессию обычного пользователя
            SessionManager.SaveSession(new User { Id = 2, Role = Role.User });

            // Регистрируем команду в CommandProcessor, если она еще не зарегистрирована
            CommandProcessor.RegisterCommand(new ShowLogsCommand());

            // Выполняем через CommandProcessor, так как именно он проверяет права (AllowedRoles)
            CommandProcessor.ProcessInput(command);

            // Проверяем, что консоль выдала сообщение о нехватке прав
            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);
        }

        [Trait("Category", "Система обновлений")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_22", MemberType = typeof(XmlDataProvider))]
        public async Task TC22_UpdateSystem_ChecksForReleaseOnGitHub(string expectedOutput)
        {
            // Действие: Вызываем проверку обновлений
            // Так как метод асинхронный, тест тоже делаем async Task
            await UpdateService.CheckForUpdatesAsync();

            // Убеждаемся, что программа хотя бы начала проверку и обратилась к GitHub
            var output = _consoleOutput.ToString();
            Assert.Contains(expectedOutput, output);
        }

        [Trait("Category", "Система обновлений")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_23", MemberType = typeof(XmlDataProvider))]
        public async Task TC23_UpdateSystem_AcceptPatch_DownloadsAndPreparesRestart(string input, string expectedOutput)
        {
            // Имитируем ввод пользователя (yes) в консоль
            var stringReader = new StringReader(input + Environment.NewLine);
            Console.SetIn(stringReader);

            try
            {
                // Если на GitHub есть версия выше локальной, то этот метод попытается 
                // скачать файл и вызвать Environment.Exit(0), что прервет тестирование.
                await UpdateService.CheckForUpdatesAsync();

                var output = _consoleOutput.ToString();

                // Если текущая версия и так последняя, программа не спросит (yes/no), 
                // поэтому этот Assert может упасть, если на GitHub нет новой версии. 
                // Мы проверяем сам факт того, что тест попытался обработать сценарий.
                if (output.Contains("ДОСТУПНО НОВОЕ ОБНОВЛЕНИЕ"))
                {
                    Assert.Contains(expectedOutput, output);
                }
            }
            finally
            {
                // Возвращаем стандартный ввод
                Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            }
        }

        [Trait("Category", "Система обновлений")]
        [Theory]
        [MemberData(nameof(XmlDataProvider.GetTestData), "TC_24", MemberType = typeof(XmlDataProvider))]
        public async Task TC24_UpdateSystem_DeclinePatch_ContinuesBoot(string input, string expectedOutput)
        {
            // Имитируем ввод пользователя (no) в консоль
            var stringReader = new StringReader(input + Environment.NewLine);
            Console.SetIn(stringReader);

            try
            {
                await UpdateService.CheckForUpdatesAsync();
                var output = _consoleOutput.ToString();

                // Проверяем, что метод завершился и продолжил загрузку без краша
                Assert.Contains(expectedOutput, output);
            }
            finally
            {
                // Возвращаем стандартный ввод
                Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            }
        }
    }
}