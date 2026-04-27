using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class RemoveTrackObjectCommand : ICommand
    {
        public string Name => "--removetrackobject";
        public string Description => "Удалить процесс из слежения. Формат: --removeTrackObject \"ИмяПроцесса\"";
        public Role[] AllowedRoles => new[] { Role.Admin, Role.Analyst };

        public void Execute(string[] args)
        {
            if (args.Length != 1) throw new CommandSyntaxException("Укажите имя процесса.");
            string processName = args[0].ToLower();

            using (var db = new AppDbContext())
            {
                var obj = db.TrackedObjects.FirstOrDefault(t => t.ProcessName == processName);
                if (obj == null) throw new NotFoundException($"Процесс '{processName}' не найден в списке слежения.");

                db.TrackedObjects.Remove(obj);
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Слежение за '{processName}' прекращено.");
                Console.ResetColor();
            }
        }
    }
}
