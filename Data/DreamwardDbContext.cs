using System;
using System.IO;
using System.Linq;
using Dreamward.Models;
using Microsoft.EntityFrameworkCore;

namespace Dreamward.Data;

/// <summary>
/// Контекст базы данных SQLite с автоматическим развёртыванием и защитой от сбоев (п. 4.2, 4.6 ТЗ)
/// </summary>
public class DreamwardDbContext : DbContext
{
    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();
    public DbSet<SaveSlot> SaveSlots => Set<SaveSlot>();
    public DbSet<PlaythroughStats> PlaythroughStats => Set<PlaythroughStats>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<HighScoreRecord> HighScores => Set<HighScoreRecord>();

    private readonly string _dbPath;

    public DreamwardDbContext()
    {
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dreamward.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    /// <summary>
    /// Надёжная инициализация БД при первом запуске или отсутствии файла SQLite (п. 4.2 ТЗ)
    /// </summary>
    public bool EnsureDatabaseInitializedSafe(out string statusMessage)
    {
        try
        {
            Database.EnsureCreated();

            if (!PlayerProfiles.Any())
            {
                PlayerProfiles.Add(new PlayerProfile { PlayerName = "Бушуев И. А. (2407г2)" });
            }

            if (!SaveSlots.Any())
            {
                SaveSlots.Add(new SaveSlot { SlotId = 1, LocationId = "WhiteNook", PlayerX = 400, PlayerY = 240 });
            }

            if (!PlaythroughStats.Any())
            {
                PlaythroughStats.Add(new PlaythroughStats { Id = 1 });
            }

            if (!Achievements.Any())
            {
                Achievements.AddRange(
                    new Achievement { Code = "FIRST_STEP", Title = "Первый шаг во сне", Description = "Исследовать локацию White Nook", IsUnlocked = true },
                    new Achievement { Code = "LOOP_WALKER", Title = "Петля Бесконечности", Description = "Бесшовно пересечь границу White Nook", IsUnlocked = false },
                    new Achievement { Code = "PEACEMAKER", Title = "Миротворец Сомниума", Description = "Пощадить противника через MERCY", IsUnlocked = false },
                    new Achievement { Code = "PANIC_SKILL", Title = "Прорыв Паники", Description = "Применить отчаянное умение при Страхе 71–100%", IsUnlocked = false }
                );
            }

            SaveChanges();
            statusMessage = "База данных SQLite (dreamward.db) успешно инициализирована.";
            return true;
        }
        catch (Exception ex)
        {
            statusMessage = $"Предупреждение СУБД (п. 4.2 ТЗ): {ex.Message}. Активирован резервный буфер в памяти.";
            return false;
        }
    }
}
