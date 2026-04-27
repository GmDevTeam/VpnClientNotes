using System;
using System.Collections.Generic;
using System.Text;

namespace VpnClientNotes.Models
{
    public class TrackedObject
    {
        public int Id { get; set; }
        public string ProcessName { get; set; } // Имя процесса, например "chrome"
    }
}
