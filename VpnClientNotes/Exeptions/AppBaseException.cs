using System;
using System.Collections.Generic;
using System.Text;
using VpnClientNotes.Services;

namespace VpnClientNotes.Exeptions
{
    /// <summary>
    /// Базовый класс для всех пользовательских исключений приложения.
    /// Автоматически логирует ошибку при её возникновении.
    /// </summary>
    public abstract class AppBaseException : Exception
    {
        protected AppBaseException(string message) : base(message)
        {
            // Сразу при создании исключения пишем его в лог
            LoggerService.LogError($"[ОШИБКА]: {this.GetType().Name} - {message}");
        }
    }

    /// <summary>
    /// Ошибка авторизации.
    /// </summary>
    public class UnauthorizedException : AppBaseException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    /// <summary>
    /// Ошибка неверной команды или синтаксиса.
    /// </summary>
    public class CommandSyntaxException : AppBaseException
    {
        public CommandSyntaxException(string message) : base(message) { }
    }
}
