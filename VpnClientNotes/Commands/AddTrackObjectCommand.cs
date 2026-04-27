using System;
using System.Linq;
using VpnClientNotes.Data;
using VpnClientNotes.Exeptions;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class AddTrackObjectCommand : ICommand
    {
        public string Name => "--addtrackobject";
        public string Description => "Добавить процесс для слежения. Формат: --addTrackObject \"ИмяПроцесса\"";
        public Role[] AllowedRoles => new[] { Role.Analyst };

        public void Execute(string[] args)
        {
            if (args.Length != 1) throw new CommandSyntaxException("Укажите имя процесса (без .exe).");
            string processName = args[0].ToLower();

            using (var db = new AppDbContext())
            {
                if (db.TrackedObjects.Any(t => t.ProcessName == processName))
                    throw new ObjectAlreadyExistsException($"Процесс '{processName}' уже отслеживается.");

                db.TrackedObjects.Add(new TrackedObject { ProcessName = processName });
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Процесс '{processName}' добавлен в WatchDog!");
                Console.ResetColor();
            }
        }
    }
}
