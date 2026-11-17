using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dreamward.Data;
using Dreamward.Models;

namespace Dreamward.ViewModels;

/// <summary>
/// Главная ViewModel игры «Dreamward (Somnium)» по шаблону MVVM (п. 4.1, 4.2, 4.5 ТЗ)
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DreamwardDbContext _db = new();
    private readonly Random _rng = new();

    public const double CanvasWidth = 800;
    public const double CanvasHeight = 480;
    public const double PlayerSize = 28;

    [ObservableProperty] private string _currentLocation = "WhiteNook";
    [ObservableProperty] private double _playerX = 400;
    [ObservableProperty] private double _playerY = 240;

    [ObservableProperty] private int _currentHP = 100;
    [ObservableProperty] private int _maxHP = 100;
    [ObservableProperty] private int _currentSP = 60;
    [ObservableProperty] private int _maxSP = 60;
    [ObservableProperty] private int _fearLevel = 12; // 0..100%

    [ObservableProperty] private bool _isInBattle;
    [ObservableProperty] private string _enemyName = "Шепчущая Тень";
    [ObservableProperty] private int _enemyHP = 100;
    [ObservableProperty] private int _enemyMaxHP = 100;
    [ObservableProperty] private int _enemyMercy = 0;

    [ObservableProperty] private string _dialogMessage = "* Добро пожаловать в мир сновидений Dreamward (Somnium).";
    [ObservableProperty] private string _battleLog = "* Вы столкнулись с тенью подсознания!";

    public ObservableCollection<InventoryItem> Inventory { get; } = new();

    /// <summary>
    /// Вычисление текущего психологического состояния по шкале Страха (п. 4.1 ТЗ)
    /// </summary>
    public FearState CurrentFearState => FearLevel switch
    {
        <= 30 => FearState.Stable,
        <= 70 => FearState.Anxiety,
        _ => FearState.Panic
    };

    public bool IsDesperateSkillUnlocked => CurrentFearState == FearState.Panic;

    public MainViewModel()
    {
        _db.EnsureDatabaseInitializedSafe(out string dbStatus);
        DialogMessage = $"* {dbStatus}";

        Inventory.Add(new InventoryItem { Id = "tea", Name = "Ромашковый настой", Category = ItemCategory.RestoreHP, EffectValue = 45, Quantity = 2 });
        Inventory.Add(new InventoryItem { Id = "stardust", Name = "Звёздная пыль", Category = ItemCategory.RestoreSP, EffectValue = 30, Quantity = 2 });
        Inventory.Add(new InventoryItem { Id = "mantra", Name = "Дыхательная мантра", Category = ItemCategory.ReduceFear, EffectValue = 30, Quantity = 2 });
    }

    /// <summary>
    /// Перемещение персонажа с бесшовным зацикливанием в White Nook и защитой границ (п. 4.1, 4.2 ТЗ)
    /// </summary>
    public void MovePlayer(double dx, double dy)
    {
        if (IsInBattle) return;

        double nextX = PlayerX + dx;
        double nextY = PlayerY + dy;

        if (CurrentLocation == "WhiteNook")
        {
            // Бесшовное зацикливание координат в стартовой локации White Nook
            if (nextX < -PlayerSize) nextX = CanvasWidth - PlayerSize;
            else if (nextX > CanvasWidth) nextX = 0;

            if (nextY < -PlayerSize) nextY = CanvasHeight - PlayerSize;
            else if (nextY > CanvasHeight) nextY = 0;
        }
        else
        {
            // Предотвращение выхода за границы экрана и застревания в стенах
            nextX = Math.Clamp(nextX, 24, CanvasWidth - 24 - PlayerSize);
            nextY = Math.Clamp(nextY, 24, CanvasHeight - 24 - PlayerSize);
        }

        PlayerX = nextX;
        PlayerY = nextY;
    }

    /// <summary>
    /// Базовая атака в пошаговом бою (ATTACK) со штрафом к точности при Тревоге (31–70%)
    /// </summary>
    [RelayCommand]
    private void Attack()
    {
        if (!IsInBattle) return;

        double accuracy = CurrentFearState == FearState.Stable ? 1.0 : 0.75;
        if (_rng.NextDouble() > accuracy)
        {
            BattleLog += "\n* [ПРОМАХ]: Тревога снизила точность вашей базовой атаки!";
        }
        else
        {
            int damage = _rng.Next(22, 31);
            EnemyHP = Math.Max(0, EnemyHP - damage);
            BattleLog += $"\n* [ATTACK]: Нанесено {damage} ед. физического урона!";
        }

        CheckBattleOutcomeOrEnemyTurn();
    }

    /// <summary>
    /// Психологическое умение (ACT / SKILL) с проверкой баланса SP (п. 4.1 и 4.2 ТЗ)
    /// </summary>
    [RelayCommand]
    private void UseSkill(string skillCode)
    {
        if (!IsInBattle) return;

        double costMultiplier = CurrentFearState == FearState.Panic ? 1.4 : 1.0;
        int baseCost = skillCode == "DESPERATE" ? 25 : 12;
        int actualCost = (int)Math.Round(baseCost * costMultiplier);

        // Требование надёжности п. 4.2: исключено аварийное завершение при 0 SP
        if (CurrentSP < actualCost)
        {
            BattleLog += $"\n* [ВНИМАНИЕ]: Недостаточно очков SP ({CurrentSP}/{actualCost} SP)!";
            return;
        }

        if (skillCode == "DESPERATE" && !IsDesperateSkillUnlocked)
        {
            BattleLog += "\n* Отчаянная способность доступна только в состоянии Паники (71–100% Страха)!";
            return;
        }

        CurrentSP -= actualCost;

        if (skillCode == "DESPERATE")
        {
            EnemyHP = Math.Max(0, EnemyHP - 75);
            FearLevel = Math.Max(0, FearLevel - 35);
            BattleLog += "\n* [ПРОРЫВ ПАНИКИ]: 75 ед. урона врагу и снижение Страха на 35%!";
        }
        else
        {
            EnemyMercy = Math.Min(100, EnemyMercy + 50);
            FearLevel = Math.Max(0, FearLevel - 20);
            BattleLog += "\n* [ACT / SKILL]: Вы успокоили дыхание (-20% Страха, Пощада врага +50%).";
        }

        OnPropertyChanged(nameof(CurrentFearState));
        OnPropertyChanged(nameof(IsDesperateSkillUnlocked));
        CheckBattleOutcomeOrEnemyTurn();
    }

    /// <summary>
    /// Применение предмета из инвентаря с защитой от пустой ячейки (п. 4.2 ТЗ)
    /// </summary>
    [RelayCommand]
    private void UseItem(InventoryItem? item)
    {
        if (item is null || item.Quantity <= 0)
        {
            DialogMessage = "* Выбранная ячейка инвентаря пуста! Применение отменено.";
            return;
        }

        item.Quantity--;
        switch (item.Category)
        {
            case ItemCategory.RestoreHP:
                CurrentHP = Math.Min(MaxHP, CurrentHP + item.EffectValue);
                break;
            case ItemCategory.RestoreSP:
                CurrentSP = Math.Min(MaxSP, CurrentSP + item.EffectValue);
                break;
            case ItemCategory.ReduceFear:
                FearLevel = Math.Max(0, FearLevel - item.EffectValue);
                OnPropertyChanged(nameof(CurrentFearState));
                OnPropertyChanged(nameof(IsDesperateSkillUnlocked));
                break;
        }

        DialogMessage = $"* Использован предмет «{item.Name}».";
    }

    /// <summary>
    /// Точка сохранения: полное восстановление ресурсов и запись в SQLite (п. 4.1 ТЗ)
    /// </summary>
    [RelayCommand]
    public void TriggerSavePoint()
    {
        CurrentHP = MaxHP;
        CurrentSP = MaxSP;

        try
        {
            var slot = _db.SaveSlots.FirstOrDefault(s => s.SlotId == 1);
            if (slot is null)
            {
                slot = new SaveSlot { SlotId = 1 };
                _db.SaveSlots.Add(slot);
            }

            slot.LocationId = CurrentLocation;
            slot.PlayerX = PlayerX;
            slot.PlayerY = PlayerY;
            slot.CurrentHP = CurrentHP;
            slot.CurrentSP = CurrentSP;
            slot.FearLevel = FearLevel;
            slot.InventoryJson = JsonSerializer.Serialize(Inventory);
            slot.UpdatedAt = DateTime.Now;

            _db.SaveChanges();
            DialogMessage = "* Ресурсы полностью восстановлены! Игровое состояние сохранено в БД SQLite.";
        }
        catch (Exception ex)
        {
            DialogMessage = $"* Ошибка сохранения в SQLite: {ex.Message}";
        }
    }

    private void CheckBattleOutcomeOrEnemyTurn()
    {
        if (EnemyHP <= 0 || EnemyMercy >= 100)
        {
            IsInBattle = false;
            DialogMessage = "* Бой успешно завершён!";
            return;
        }

        double incomingMult = CurrentFearState == FearState.Panic ? 1.35 : 1.0;
        int enemyDmg = (int)Math.Round(16 * incomingMult);
        CurrentHP = Math.Max(1, CurrentHP - enemyDmg);
        FearLevel = Math.Min(100, FearLevel + 14);
        OnPropertyChanged(nameof(CurrentFearState));
        OnPropertyChanged(nameof(IsDesperateSkillUnlocked));
    }
}
