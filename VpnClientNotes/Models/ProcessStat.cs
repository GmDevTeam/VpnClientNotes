using System;
using System.Collections.Generic;
using System.Text;

namespace VpnClientNotes.Models
{
    public class ProcessStat
    {
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public double RamUsageMB { get; set; } // Сколько оперативки съел
        public DateTime RecordedAt { get; set; }
    }
}
