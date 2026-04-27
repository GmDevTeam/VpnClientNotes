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

    /// <summary>
    /// Ошибка: Запрашиваемый объект не найден в базе данных.
    /// </summary>
    public class NotFoundException : AppBaseException
    {
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Ошибка: Пользователь с таким логином уже существует.
    /// </summary>
    public class UserAlreadyExistsException : AppBaseException
    {
        public UserAlreadyExistsException(string message) : base(message) { }
    }

    /// <summary>
    /// Ошибка: Пароль не соответствует требованиям безопасности.
    /// </summary>
    public class PasswordComplexityException : AppBaseException
    {
        public PasswordComplexityException(string message) : base(message) { }
    }

    /// <summary>
    /// Ошибка: Профиль пользователя заморожен администратором.
    /// </summary>
    public class UserBannedException : AppBaseException
    {
        public UserBannedException(string message) : base(message) { }
    }

    /// <summary>
    /// Ошибка: Объект уже существует в системе (для WatchDog).
    /// </summary>
    public class ObjectAlreadyExistsException : AppBaseException
    {
        public ObjectAlreadyExistsException(string message) : base(message) { }
    }
}
