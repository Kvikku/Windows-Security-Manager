using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Core service that manages security settings: enable, disable, report, and list operations.
/// </summary>
public class SecuritySettingsManager
{
    private readonly IRegistryService _registryService;
    private readonly List<SecuritySetting> _settings;

    public SecuritySettingsManager(IRegistryService registryService, IEnumerable<ISecuritySettingProvider> providers)
    {
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        _settings = (providers ?? throw new ArgumentNullException(nameof(providers)))
            .SelectMany(p => p.GetSettings())
            .ToList();
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
        return _settings.Count;
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
