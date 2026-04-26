using System;
using System.Collections.Generic;
using System.Text;
using VpnClientNotes.Commands;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    public class HelpCommand : ICommand
    {
        public string Name => "--help";
        public string Description => "Вывод карты команд и инструкций.";

        // null означает, что команда доступна всем (гостям, юзерам, админам)
        public Role[] AllowedRoles => null;

        public void Execute(string[] args)
        {
            Console.WriteLine("\n### Карта команд VPN Notes Client ###");
            Console.WriteLine("Формат использования: [команда] [аргументы в кавычках]\n");

            foreach (var cmd in CommandProcessor.GetAllCommands())
            {
                // Формируем красивый вывод
                string roles = cmd.AllowedRoles == null ? "Все" : string.Join(", ", cmd.AllowedRoles);
                Console.WriteLine($"- **{cmd.Name}** : {cmd.Description} *(Доступ: {roles})*");
            }

            Console.WriteLine("\nПример использования:");
            Console.WriteLine("> --login \"myLogin\" \"myPassword\"");
            Console.WriteLine("> --addNewNote \"Текст моей важной заметки\"");
            Console.WriteLine("> --exit\n");
        }
    }
}
