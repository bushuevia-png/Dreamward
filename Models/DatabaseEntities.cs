using System;
using System.ComponentModel.DataAnnotations;

namespace Dreamward.Models;

/// <summary>
/// Диапазоны шкалы «Уровень Страха» (п. 4.1 ТЗ)
/// </summary>
public enum FearState
{
    Stable = 0,   // 0–30%: Стабильное состояние
    Anxiety = 1,  // 31–70%: Тревога (штраф к точности базовых атак -25%)
    Panic = 2     // 71–100%: Паника (рост входящего урона +35%, стоимости SP +40%, открытие отчаянной способности)
}

/// <summary>
/// Категории расходных предметов инвентаря (п. 4.1 ТЗ)
/// </summary>
public enum ItemCategory
{
    RestoreHP,
    RestoreSP,
    ReduceFear
}

/// <summary>
/// 1. Сущность профиля игрока в БД SQLite
/// </summary>
public class PlayerProfile
{
    [Key]
    public int Id { get; set; }
    public string PlayerName { get; set; } = "Бушуев И. А. (2407г2)";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 2. Сущность слота сохранения в БД SQLite (п. 4.1 ТЗ)
/// </summary>
public class SaveSlot
{
    [Key]
    public int SlotId { get; set; }
    public string LocationId { get; set; } = "WhiteNook";
    public double PlayerX { get; set; } = 400;
    public double PlayerY { get; set; } = 240;
    public int CurrentHP { get; set; } = 100;
    public int MaxHP { get; set; } = 100;
    public int CurrentSP { get; set; } = 60;
    public int MaxSP { get; set; } = 60;
    public int FearLevel { get; set; } = 10;
    public string InventoryJson { get; set; } = "[]";
    public string StoryFlagsJson { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 3. Сущность статистики прохождений
/// </summary>
public class PlaythroughStats
{
    [Key]
    public int Id { get; set; }
    public int BattlesWon { get; set; }
    public int EnemiesSpared { get; set; }
    public int ItemsUsed { get; set; }
    public int PeakFearLevel { get; set; }
}

/// <summary>
/// 4. Сущность системы достижений
/// </summary>
public class Achievement
{
    [Key]
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
}

/// <summary>
/// 5. Сущность таблицы рекордов
/// </summary>
public class HighScoreRecord
{
    [Key]
    public int Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime AchievedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Модель расходного предмета инвентаря
/// </summary>
public class InventoryItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public int EffectValue { get; set; }
    public int Quantity { get; set; }
}
