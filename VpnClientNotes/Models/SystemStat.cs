using System;
using System.Collections.Generic;
using System.Text;

namespace VpnClientNotes.Models
{
    /// <summary>
    /// Сущность для хранения статистики системы (собирается WatchDog-ом).
    /// </summary>
    public class SystemStat
    {
        public int Id { get; set; }
        public double CpuUsagePercent { get; set; }
        public double RamAvailableMB { get; set; }
        public double HddAvailableGB { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
