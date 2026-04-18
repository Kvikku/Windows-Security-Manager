using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Core service that manages security settings: enable, disable, report, and list operations.
/// </summary>
public class SecuritySettingsManager
{
    private readonly IRegistryService _registryService;
    private readonly List<SecuritySetting> _settings;
    private AuditLogger? _auditLogger;

    public SecuritySettingsManager(IRegistryService registryService, IEnumerable<ISecuritySettingProvider> providers)
    {
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        _settings = (providers ?? throw new ArgumentNullException(nameof(providers)))
            .SelectMany(p => p.GetSettings())
            .ToList();

        // Enrich settings with impact + consequences metadata so the UI can display
        // compatibility guidance alongside each setting at runtime.
        SettingConsequencesCatalog.Enrich(_settings);
    }

    /// <summary>
    /// Attaches an audit logger to record all enable/disable actions.
    /// </summary>
    public void SetAuditLogger(AuditLogger logger)
    {
        _auditLogger = logger;
    }

    /// <summary>
    /// Gets all registered settings, optionally filtered by category.
    /// </summary>
    public IReadOnlyList<SecuritySetting> GetSettings(SecurityCategory? category = null)
    {
        if (category.HasValue)
            return _settings.Where(s => s.Category == category.Value).ToList();
        return _settings;
    }

    /// <summary>
    /// Searches settings by keyword across ID, name, and description.
    /// </summary>
    public IReadOnlyList<SecuritySetting> SearchSettings(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return _settings;

        return _settings.Where(s =>
            s.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Enables (hardens) a single setting by its ID.
    /// </summary>
    public bool EnableSetting(string settingId)
    {
        var setting = _settings.FirstOrDefault(s =>
            s.Id.Equals(settingId, StringComparison.OrdinalIgnoreCase));
        if (setting == null) return false;

        _registryService.SetValue(
            setting.RegistryHive,
            setting.RegistryPath,
            setting.ValueName,
            setting.EnabledValue,
            setting.ValueType);

        _auditLogger?.Log("Enable", setting.Id, $"Set {setting.Name} to {setting.EnabledValue}");
        return true;
    }

    /// <summary>
    /// Disables (unhardens) a single setting by its ID.
    /// </summary>
    public bool DisableSetting(string settingId)
    {
        var setting = _settings.FirstOrDefault(s =>
            s.Id.Equals(settingId, StringComparison.OrdinalIgnoreCase));
        if (setting == null) return false;

        _registryService.SetValue(
            setting.RegistryHive,
            setting.RegistryPath,
            setting.ValueName,
            setting.DisabledValue,
            setting.ValueType);

        _auditLogger?.Log("Disable", setting.Id, $"Set {setting.Name} to {setting.DisabledValue}");
        return true;
    }

    /// <summary>
    /// Enables all settings in a category.
    /// </summary>
    public int EnableCategory(SecurityCategory category)
    {
        var categorySettings = _settings.Where(s => s.Category == category).ToList();
        foreach (var setting in categorySettings)
        {
            _registryService.SetValue(
                setting.RegistryHive,
                setting.RegistryPath,
                setting.ValueName,
                setting.EnabledValue,
                setting.ValueType);
        }

        _auditLogger?.Log("EnableCategory", category.ToString(), $"Enabled {categorySettings.Count} settings");
        return categorySettings.Count;
    }

    /// <summary>
    /// Disables all settings in a category.
    /// </summary>
    public int DisableCategory(SecurityCategory category)
    {
        var categorySettings = _settings.Where(s => s.Category == category).ToList();
        foreach (var setting in categorySettings)
        {
            _registryService.SetValue(
                setting.RegistryHive,
                setting.RegistryPath,
                setting.ValueName,
                setting.DisabledValue,
                setting.ValueType);
        }

        _auditLogger?.Log("DisableCategory", category.ToString(), $"Disabled {categorySettings.Count} settings");
        return categorySettings.Count;
    }

    /// <summary>
    /// Enables all registered settings.
    /// </summary>
    public int EnableAll()
    {
        foreach (var setting in _settings)
        {
            _registryService.SetValue(
                setting.RegistryHive,
                setting.RegistryPath,
                setting.ValueName,
                setting.EnabledValue,
                setting.ValueType);
        }

        _auditLogger?.Log("EnableAll", "All settings", $"Enabled {_settings.Count} settings");
        return _settings.Count;
    }

    /// <summary>
    /// Disables all registered settings.
    /// </summary>
    public int DisableAll()
    {
        foreach (var setting in _settings)
        {
            _registryService.SetValue(
                setting.RegistryHive,
                setting.RegistryPath,
                setting.ValueName,
                setting.DisabledValue,
                setting.ValueType);
        }

        _auditLogger?.Log("DisableAll", "All settings", $"Disabled {_settings.Count} settings");
        return _settings.Count;
    }

    /// <summary>
    /// Enables specific settings by their IDs.
    /// </summary>
    public int EnableSettings(IEnumerable<string> settingIds)
    {
        int count = 0;
        foreach (var id in settingIds)
        {
            if (EnableSetting(id)) count++;
        }
        return count;
    }

    /// <summary>
    /// Disables specific settings by their IDs.
    /// </summary>
    public int DisableSettings(IEnumerable<string> settingIds)
    {
        int count = 0;
        foreach (var id in settingIds)
        {
            if (DisableSetting(id)) count++;
        }
        return count;
    }

    /// <summary>
    /// Previews what would change for an enable operation without writing to the registry.
    /// </summary>
    public IReadOnlyList<DryRunChange> DryRunEnable(IEnumerable<SecuritySetting> settings)
    {
        return settings.Select(s =>
        {
            var currentValue = _registryService.GetValue(s.RegistryHive, s.RegistryPath, s.ValueName);
            return new DryRunChange
            {
                Setting = s,
                CurrentValue = currentValue,
                NewValue = s.EnabledValue,
                IsCurrentlyConfigured = currentValue != null
            };
        }).ToList();
    }

    /// <summary>
    /// Previews what would change for a disable operation without writing to the registry.
    /// </summary>
    public IReadOnlyList<DryRunChange> DryRunDisable(IEnumerable<SecuritySetting> settings)
    {
        return settings.Select(s =>
        {
            var currentValue = _registryService.GetValue(s.RegistryHive, s.RegistryPath, s.ValueName);
            return new DryRunChange
            {
                Setting = s,
                CurrentValue = currentValue,
                NewValue = s.DisabledValue,
                IsCurrentlyConfigured = currentValue != null
            };
        }).ToList();
    }

    /// <summary>
    /// Gets the status of a single setting.
    /// </summary>
    public SettingStatus GetSettingStatus(SecuritySetting setting)
    {
        var currentValue = _registryService.GetValue(
            setting.RegistryHive,
            setting.RegistryPath,
            setting.ValueName);

        bool isConfigured = currentValue != null;
        bool isEnabled = isConfigured && ValuesMatch(currentValue, setting.EnabledValue);
        bool matchesRecommended = isConfigured && ValuesMatch(currentValue, setting.RecommendedValue);

        return new SettingStatus
        {
            Setting = setting,
            CurrentValue = currentValue,
            IsEnabled = isEnabled,
            IsConfigured = isConfigured,
            MatchesRecommended = matchesRecommended
        };
    }

    /// <summary>
    /// Generates a compliance report for all settings or a specific category.
    /// </summary>
    public SecurityReport GenerateReport(SecurityCategory? category = null)
    {
        var settingsToReport = category.HasValue
            ? _settings.Where(s => s.Category == category.Value)
            : _settings;

        var statuses = settingsToReport
            .Select(GetSettingStatus)
            .ToList();

        return new SecurityReport { Settings = statuses };
    }

    private static bool ValuesMatch(object? current, object? expected)
    {
        if (current == null && expected == null) return true;
        if (current == null || expected == null) return false;

        // Handle numeric comparisons (registry may return int for DWORD)
        if (current is int currentInt && expected is int expectedInt)
            return currentInt == expectedInt;

        if (current is long currentLong && expected is long expectedLong)
            return currentLong == expectedLong;

        // Handle byte array comparisons
        if (current is byte[] currentBytes && expected is byte[] expectedBytes)
            return currentBytes.SequenceEqual(expectedBytes);

        // Handle string array comparisons
        if (current is string[] currentStrings && expected is string[] expectedStrings)
            return currentStrings.SequenceEqual(expectedStrings);

        return current.Equals(expected);
    }
}
