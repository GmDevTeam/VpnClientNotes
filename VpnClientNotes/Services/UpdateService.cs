using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VpnClientNotes.Services
{
    public static class UpdateService
    {
        // Указание репозитория!
        private const string GitHubRepo = "GmDevTeam/VpnClientNotes";

        // ДИНАМИЧЕСКОЕ ЧТЕНИЕ ВЕРСИИ: программа берет версию прямо из свойств своего .exe файла
        public static string CurrentVersion
        {
            get
            {
                // Получаем версию сборки (например, 1.1.0) и добавляем букву "v" спереди для GitHub
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"v{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        public static async Task CheckForUpdatesAsync()
        {
            Console.WriteLine($"[Система] Текущая версия: {CurrentVersion}");
            Console.WriteLine("[Система] Проверка обновлений на GitHub...");

            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "VpnNotesClient-Updater");

                string apiUrl = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[Система] Нет доступа к репозиторию (или релизы не найдены).\n");
                    return;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);

                string latestVersion = doc.RootElement.GetProperty("tag_name").GetString();

                // Сравниваем версии
                if (latestVersion != CurrentVersion)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n*** ДОСТУПНО НОВОЕ ОБНОВЛЕНИЕ: {latestVersion} ***");
                    Console.Write("Хотите скачать и установить обновление? (Да/Нет): ");
                    Console.ResetColor();

                    string answer = Console.ReadLine()?.ToLower();
                    if (answer == "да" || answer == "yes" || answer == "y")
                    {
                        var assets = doc.RootElement.GetProperty("assets");
                        if (assets.GetArrayLength() > 0)
                        {
                            string downloadUrl = assets[0].GetProperty("browser_download_url").GetString();
                            await PerformUpdateAsync(downloadUrl);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[Система] У вас установлена самая последняя версия.\n");
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Ошибка обновления: {ex.Message}");
            }
        }

        private static async Task PerformUpdateAsync(string downloadUrl)
        {
            Console.WriteLine("Скачивание файла с GitHub... Пожалуйста, подождите.");

            string tempFilePath = Path.Combine(Path.GetTempPath(), "VpnClientNotes_new.exe");

            using (HttpClient client = new HttpClient())
            {
                byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempFilePath, fileBytes);
            }

            Console.WriteLine("Скачивание завершено. Перезапуск системы...");

            string currentExePath = Environment.ProcessPath;
            string currentDirectory = Path.GetDirectoryName(currentExePath);
            string batFilePath = Path.Combine(currentDirectory, "updater.bat");

            // 1. Команда chcp 65001 заставляет CMD работать в кодировке UTF-8.
            // 2. Использование timeout вместо ping (более стабильно).
            string batContent = $@"
                                @echo off
                                chcp 65001 > nul
                                timeout /t 2 /nobreak > nul
                                copy /Y ""{tempFilePath}"" ""{currentExePath}""
                                start """" ""{currentExePath}""
                                del ""{tempFilePath}""
                                del ""%~f0""
                                ";
            // Явно сохраняем .bat файл в UTF-8
            File.WriteAllText(batFilePath, batContent, Encoding.UTF8);

            var processInfo = new ProcessStartInfo("cmd.exe", $"/c \"{batFilePath}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(processInfo);

            Environment.Exit(0);
        }
    }
}
