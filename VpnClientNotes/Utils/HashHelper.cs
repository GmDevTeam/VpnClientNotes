using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace VpnClientNotes.Utils
{
    /// <summary>
    /// Утилита для хэширования строк (паролей).
    /// </summary>
    public static class HashHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            // Создаем экземпляр SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Вычисляем хэш - получаем массив байтов
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Преобразуем массив байтов в строку (шестнадцатеричный формат)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
