using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace VpnClientNotes.Services
{
    public static class UpdateService
    {
        // Наша текущая версия
        public const string CurrentVersion = "v1.0.0";

        // ВАЖНО: Замени "ТвойЛогин" и "ИмяРепозитория" на свои из GitHub!
        // Например: "Oldman/VpnClientNotes"
        private const string GitHubRepo = "ТвойЛогин/ИмяРепозитория";

        /// <summary>
        /// Асинхронная проверка обновлений через GitHub API.
        /// </summary>
        public static async Task CheckForUpdatesAsync()
        {
            Console.WriteLine($"[Система] Текущая версия: {CurrentVersion}");
            Console.WriteLine("[Система] Проверка обновлений на GitHub...");

            try
            {
                using HttpClient client = new HttpClient();
                // GitHub API требует обязательный заголовок User-Agent
                client.DefaultRequestHeaders.Add("User-Agent", "VpnNotesClient-Updater");

                string apiUrl = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[Система] Нет доступа к репозиторию или релизы еще не созданы.\n");
                    return;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);

                // Получаем версию последнего релиза (tag_name)
                string latestVersion = doc.RootElement.GetProperty("tag_name").GetString();

                // Сравниваем версии (например, v1.0.0 и v1.1.0)
                if (latestVersion != CurrentVersion)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n*** ДОСТУПНО НОВОЕ ОБНОВЛЕНИЕ: {latestVersion} ***");
                    Console.Write("Хотите скачать и установить обновление? (Да/Нет): ");
                    Console.ResetColor();

                    string answer = Console.ReadLine()?.ToLower();
                    if (answer == "да" || answer == "yes" || answer == "y")
                    {
                        // Ищем ссылку на скачивание файла в массиве assets
                        var assets = doc.RootElement.GetProperty("assets");
                        if (assets.GetArrayLength() > 0)
                        {
                            string downloadUrl = assets[0].GetProperty("browser_download_url").GetString();
                            await PerformUpdateAsync(downloadUrl);
                        }
                        else
                        {
                            Console.WriteLine("[Ошибка] В релизе на GitHub нет прикрепленного .exe файла.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[Система] У вас установлена самая актуальная версия.\n");
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Ошибка при проверке обновлений GitHub: {ex.Message}");
                Console.WriteLine("[Система] Ошибка сети при проверке обновлений.\n");
            }
        }

        private static async Task PerformUpdateAsync(string downloadUrl)
        {
            Console.WriteLine("Скачивание файла с GitHub... Пожалуйста, подождите.");

            string tempFilePath = Path.Combine(Path.GetTempPath(), "VpnClientNotes_new.exe");

            // Скачиваем новый .exe файл во временную папку Windows
            using (HttpClient client = new HttpClient())
            {
                byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempFilePath, fileBytes);
            }

            Console.WriteLine("Скачивание завершено. Перезапуск системы...");

            string currentExePath = Assembly.GetExecutingAssembly().Location;
            currentExePath = Path.ChangeExtension(currentExePath, ".exe");
            string currentDirectory = Path.GetDirectoryName(currentExePath);
            string batFilePath = Path.Combine(currentDirectory, "updater.bat");

            // Скрипт копирует скачанный файл из Temp поверх нашего и запускает
            string batContent = $@"
                                @echo off
                                ping 127.0.0.1 -n 3 > nul
                                copy /Y ""{tempFilePath}"" ""{currentExePath}""
                                start """" ""{currentExePath}""
                                del ""{tempFilePath}""
                                del ""%~f0""
                                ";
            File.WriteAllText(batFilePath, batContent);

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
