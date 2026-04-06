using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Service for backing up and restoring security setting registry values.
/// </summary>
public class BackupService
{
    private readonly SecuritySettingsManager _manager;
    private readonly IRegistryService _registryService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public BackupService(SecuritySettingsManager manager, IRegistryService registryService)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
    }

    /// <summary>
    /// Creates a backup of all current registry values for managed settings.
    /// </summary>
    public BackupData CreateBackup(SecurityCategory? category = null)
    {
        var settings = _manager.GetSettings(category);
        var entries = new List<BackupEntry>();

        foreach (var setting in settings)
        {
            var currentValue = _registryService.GetValue(
                setting.RegistryHive,
                setting.RegistryPath,
                setting.ValueName);

            entries.Add(new BackupEntry
            {
                SettingId = setting.Id,
                RegistryHive = setting.RegistryHive,
                RegistryPath = setting.RegistryPath,
                ValueName = setting.ValueName,
                ValueType = setting.ValueType,
                Value = currentValue,
                WasConfigured = currentValue != null
            });
        }

        return new BackupData { Entries = entries };
    }

    /// <summary>
    /// Saves a backup to a JSON file.
    /// </summary>
    public void SaveToFile(BackupData backup, string filePath)
    {
        // Convert to serializable format (object values aren't always directly serializable)
        var serializable = new
        {
            backup.CreatedAt,
            Entries = backup.Entries.Select(e => new
            {
                e.SettingId,
                e.RegistryHive,
                e.RegistryPath,
                e.ValueName,
                ValueType = e.ValueType.ToString(),
                Value = e.Value?.ToString(),
                e.WasConfigured
            })
        };

        var json = JsonSerializer.Serialize(serializable, JsonOptions);

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Restores settings from a backup file. Only entries matching known settings are restored.
    /// </summary>
    public int RestoreFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Backup file not found: {filePath}");

        // Build a lookup of known settings by ID for validation
        var knownSettings = _manager.GetSettings()
            .ToDictionary(s => s.Id, s => s, StringComparer.OrdinalIgnoreCase);

        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        int restored = 0;
        var entries = root.GetProperty("Entries");

        foreach (var entry in entries.EnumerateArray())
        {
            var settingId = entry.GetProperty("SettingId").GetString();
            if (settingId == null) continue;

            // Validate against known settings — reject entries that don't match
            if (!knownSettings.TryGetValue(settingId, out var knownSetting))
                continue;

            var wasConfigured = entry.GetProperty("WasConfigured").GetBoolean();

            if (wasConfigured)
            {
                var valueStr = entry.GetProperty("Value").GetString();
                var valueTypeStr = entry.GetProperty("ValueType").GetString();
                if (valueStr == null || valueTypeStr == null) continue;

                var valueType = Enum.Parse<SettingValueType>(valueTypeStr);

                object value = valueType switch
                {
                    SettingValueType.DWord => int.Parse(valueStr),
                    SettingValueType.QWord => long.Parse(valueStr),
                    _ => valueStr
                };

                // Use the known setting's registry path instead of trusting the backup file
                _registryService.SetValue(
                    knownSetting.RegistryHive,
                    knownSetting.RegistryPath,
                    knownSetting.ValueName,
                    value,
                    valueType);
                restored++;
            }
            else
            {
                // The value didn't exist before — delete it to restore using the known path
                _registryService.DeleteValue(
                    knownSetting.RegistryHive,
                    knownSetting.RegistryPath,
                    knownSetting.ValueName);
                restored++;
            }
        }

        return restored;
    }
}
