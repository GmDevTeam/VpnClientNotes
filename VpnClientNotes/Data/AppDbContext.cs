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
        public DbSet<WatchDogSetting> WatchDogSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Trusted_Connection=True означает вход по Windows авторизации.
            // TrustServerCertificate=True нужен для локального сервера без SSL.
            string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=VpnNotesDb;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Сидирование пользователя
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Login = "admin", PasswordHash = "admin", Role = Role.Admin }
            );

            // Сидирование настроек WatchDog (по умолчанию включено всё, интервал 10 секунд)
            modelBuilder.Entity<WatchDogSetting>().HasData(
                new WatchDogSetting
                {
                    Id = 1,
                    IsActive = true,
                    TrackCpu = true,
                    TrackRam = true,
                    TrackHdd = true,
                    IntervalSeconds = 10
                }
            );
        }
    }
}
