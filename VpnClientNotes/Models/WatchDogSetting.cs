using System;
using System.Collections.Generic;
using System.Text;

namespace VpnClientNotes.Models
{
    /// <summary>
    /// Сущность для хранения настроек сервиса WatchDog в базе данных.
    /// Аналитик сможет менять эти флаги.
    /// </summary>
    public class WatchDogSetting
    {
        public int Id { get; set; }

        public bool IsActive { get; set; } // Включен ли WatchDog вообще

        public bool TrackCpu { get; set; } // Следить ли за процессором
        public bool TrackRam { get; set; } // Следить ли за ОЗУ
        public bool TrackHdd { get; set; } // Следить ли за дисками

        public int IntervalSeconds { get; set; } // Интервал сбора (по умолчанию 10 сек)
    }
}
