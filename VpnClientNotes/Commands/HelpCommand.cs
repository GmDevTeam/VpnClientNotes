using System;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class HelpCommand : ICommand
    {
        public string Name => "--help";
        public string Description => "Вывод карты команд и инструкций.";
        public Role[] AllowedRoles => null;

        public void Execute(string[] args)
        {
            Console.WriteLine("\n# Карта команд VPN Notes Client\n");
            Console.WriteLine("Формат использования: `[команда] [аргументы в кавычках]`\n");
            Console.WriteLine("## Доступные команды:\n");

            foreach (var cmd in CommandProcessor.GetAllCommands())
            {
                string roles = cmd.AllowedRoles == null ? "Все (включая Гостей)" : string.Join(", ", cmd.AllowedRoles);

                // Красивый Markdown список
                Console.WriteLine($"* **`{cmd.Name}`**");
                Console.WriteLine($"  > {cmd.Description}");
                Console.WriteLine($"  > *Доступ:* `{roles}`\n");
            }

            Console.WriteLine("## Примеры использования:");
            Console.WriteLine("```bash");
            Console.WriteLine("> --register \"Student\" \"P@ssw0rd123\"");
            Console.WriteLine("> --login \"Student\" \"P@ssw0rd123\"");
            Console.WriteLine("> --addNewNote \"Текст моей важной заметки\"");
            Console.WriteLine("> --logout");
            Console.WriteLine("> --exit");
            Console.WriteLine("```\n");
        }
    }
}
