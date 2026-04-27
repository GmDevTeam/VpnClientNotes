using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using VpnClientNotes.Data;
using VpnClientNotes.Models;

namespace VpnClientNotes.Services
{
    /// <summary>
    /// Сервис аналитики. Работает в фоновом потоке, собирает статистику 
    /// системы и записывает в БД согласно настройкам.
    /// </summary>
    public static class WatchDogService
    {
        private static System.Timers.Timer _timer;
        private static PerformanceCounter _cpuCounter;
        private static PerformanceCounter _ramCounter;

        public static void Start()
        {
            try
            {
                // Инициализируем системные счетчики Windows (CPU и RAM)
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                // Первый вызов счетчика CPU всегда дает 0, делаем "холостой" вызов
                _cpuCounter.NextValue();
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Ошибка инициализации счетчиков WatchDog: {ex.Message}");
            }

            // Создаем таймер с дефолтным интервалом (10 секунд = 10000 миллисекунд)
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += OnTimedEvent; // Привязываем метод, который будет срабатывать по таймеру
            _timer.AutoReset = true; // Зацикливаем таймер
            _timer.Enabled = true;   // Запускаем

            LoggerService.LogInfo("WatchDog Service запущен в фоновом режиме.");
        }

        /// <summary>
        /// Этот метод срабатывает автоматически каждые N секунд.
        /// </summary>
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                // Подключаемся к БД (каждый тик таймера создает новое подключение)
                using (var db = new AppDbContext())
                {
                    // Получаем настройки из БД
                    var settings = db.WatchDogSettings.FirstOrDefault();

                    // Если настроек нет или WatchDog выключен аналитиком - ничего не делаем
                    if (settings == null || !settings.IsActive) return;

                    // Если аналитик изменил интервал в БД - обновляем таймер "на лету"
                    if (_timer.Interval != settings.IntervalSeconds * 1000)
                    {
                        _timer.Interval = settings.IntervalSeconds * 1000;
                    }

                    // Подготавливаем объект статистики
                    var stat = new SystemStat { RecordedAt = DateTime.Now };

                    // Сбор CPU
                    if (settings.TrackCpu && _cpuCounter != null)
                    {
                        stat.CpuUsagePercent = Math.Round(_cpuCounter.NextValue(), 2);
                    }

                    // Сбор ОЗУ (в Мегабайтах)
                    if (settings.TrackRam && _ramCounter != null)
                    {
                        stat.RamAvailableMB = Math.Round(_ramCounter.NextValue(), 2);
                    }

                    // Сбор свободного места на диске (в Гигабайтах)
                    if (settings.TrackHdd)
                    {
                        // Берем первый доступный жесткий диск (обычно это диск C:)
                        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
                        if (drive != null)
                        {
                            stat.HddAvailableGB = Math.Round((double)drive.AvailableFreeSpace / (1024 * 1024 * 1024), 2);
                        }
                    }

                    // === СБОР ДАННЫХ ДИНАМИЧЕСКИХ ОБЪЕКТОВ ===
                    var trackedObjects = db.TrackedObjects.ToList();
                    foreach (var trackedObj in trackedObjects)
                    {
                        // Ищем все запущенные процессы с таким именем в Windows
                        var processes = Process.GetProcessesByName(trackedObj.ProcessName);
                        if (processes.Length > 0)
                        {
                            // Считаем общую память всех таких процессов (в Мегабайтах)
                            double totalRam = processes.Sum(p => p.WorkingSet64) / (1024.0 * 1024.0);

                            db.ProcessStats.Add(new ProcessStat
                            {
                                ProcessName = trackedObj.ProcessName,
                                RamUsageMB = Math.Round(totalRam, 2),
                                RecordedAt = DateTime.Now
                            });
                        }
                    }

                    // Сохраняем собранные данные в БД
                    db.SystemStats.Add(stat);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Если произошла ошибка (например, БД недоступна), программа не вылетит, а запишет в лог
                LoggerService.LogError($"Ошибка в фоновом процессе WatchDog: {ex.Message}");
            }
        }
    }
}
