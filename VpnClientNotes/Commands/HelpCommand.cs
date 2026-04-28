using System;
using System.Linq;
using VpnClientNotes.Models;
using VpnClientNotes.Utils;

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
            Console.WriteLine("## Доступные ВАМ команды:\n");

            // Узнаем, кто сейчас запрашивает help
            bool isAuth = SessionManager.IsAuthenticated();
            Role? currentRole = isAuth ? SessionManager.GetCurrentUserRole() : (Role?)null;

            foreach (var cmd in CommandProcessor.GetAllCommands())
            {
                bool canShow = false;

                // 1. Если команда доступна всем (AllowedRoles == null) - показываем
                if (cmd.AllowedRoles == null)
                {
                    canShow = true;
                }
                // 2. Если юзер авторизован и его роль есть в списке разрешенных - показываем
                else if (isAuth && cmd.AllowedRoles.Contains(currentRole.Value))
                {
                    canShow = true;
                }

                // Выводим только те команды, к которым есть доступ
                if (canShow)
                {
                    string roles = cmd.AllowedRoles == null ? "Все (включая Гостей)" : string.Join(", ", cmd.AllowedRoles);
                    Console.WriteLine($"* **`{cmd.Name}`**");
                    Console.WriteLine($"  > {cmd.Description}");
                    Console.WriteLine($"  > *Доступ:* `{roles}`\n");
                }
            }

            // Блок примеров остается неизменным
            Console.WriteLine("## Примеры использования:");
            Console.WriteLine("```bash");
            Console.WriteLine("> --register \"Student\" \"P@ssw0rd123\"");
            Console.WriteLine("> --login \"admin\" \"admin\"");
            Console.WriteLine("> --addNewNote \"Текст моей важной заметки\"");
            Console.WriteLine("> --showNotes");
            Console.WriteLine("> --logout");
            Console.WriteLine("> --exit");
            Console.WriteLine("```\n");
        }
    }
}
