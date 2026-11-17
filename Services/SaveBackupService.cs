using System;
using System.IO;
using System.Text.Json;
using Dreamward.Models;

namespace Dreamward.Services;

/// <summary>
/// Сервис резервного копирования слотов сохранения в формат JSON (System.Text.Json)
/// с защитой от повреждённых файлов (п. 4.2 ТЗ).
/// </summary>
public static class SaveBackupService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static bool TryExportSlotToFile(SaveSlot slot, string filePath, out string message)
    {
        try
        {
            string json = JsonSerializer.Serialize(slot, JsonOptions);
            File.WriteAllText(filePath, json);
            message = $"Сохранение успешно экспортировано в файл: {Path.GetFileName(filePath)}";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Ошибка при экспорте JSON: {ex.Message}";
            return false;
        }
    }

    public static bool TryImportSlotFromFile(string filePath, out SaveSlot? importedSlot, out string message)
    {
        importedSlot = null;
        try
        {
            if (!File.Exists(filePath))
            {
                message = "Указанный файл резервной копии не найден.";
                return false;
            }

            string json = File.ReadAllText(filePath);
            var slot = JsonSerializer.Deserialize<SaveSlot>(json, JsonOptions);

            if (slot is null || string.IsNullOrWhiteSpace(slot.LocationId) || slot.MaxHP <= 0)
            {
                message = "Файл JSON имеет некорректную структуру сохранения Dreamward.";
                return false;
            }

            importedSlot = slot;
            message = "Резервная копия JSON успешно проверена и импортирована!";
            return true;
        }
        catch (JsonException)
        {
            // Требование п. 4.2 ТЗ: отсутствие аварийного завершения при импорте повреждённого файла JSON
            message = "Ошибка импорта: файл JSON повреждён или нарушен его синтаксис. Импорт отменён.";
            return false;
        }
        catch (Exception ex)
        {
            message = $"Непредвиденная ошибка чтения файла: {ex.Message}";
            return false;
        }
    }
}
