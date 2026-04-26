using System;
using System.Collections.Generic;
using System.Text;
using VpnClientNotes.Models;
using Microsoft.EntityFrameworkCore;

namespace VpnClientNotes.Data
{
    /// <summary>
    /// Контекст базы данных Entity Framework.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<SystemStat> SystemStats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Trusted_Connection=True означает вход по Windows авторизации.
            // TrustServerCertificate=True нужен для локального сервера без SSL.
            string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=VpnNotesDb;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Сидирование (начальные данные). Создаем дефолтного админа при создании БД.
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Login = "admin", PasswordHash = "admin", Role = Role.Admin }
            // Примечание: тут должен быть хэш, но для начала оставил так
            );
        }
    }
}
