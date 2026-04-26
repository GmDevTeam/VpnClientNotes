using System;
using System.Collections.Generic;
using System.Text;
using VpnClientNotes.Models;

namespace VpnClientNotes.Commands
{
    /// <summary>
    /// Единый интерфейс для всех команд приложения.
    /// </summary>
    public interface ICommand
    {
        // Название команды, например "--addNewNote"
        string Name { get; }

        // Описание для команды --help
        string Description { get; }

        // Список ролей, которым доступна команда. Если null - доступна всем (в т.ч. гостям).
        Role[] AllowedRoles { get; }

        // Метод выполнения команды. Принимает аргументы (например, текст заметки).
        void Execute(string[] args);
    }
}
