namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents a single security setting that maps to a Windows registry value.
/// </summary>
public class SecuritySetting
{
    /// <summary>
    /// Unique identifier for this setting (e.g., "DEFENDER-001").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable name of the setting.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of what this setting does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The category this setting belongs to.
    /// </summary>
    public required SecurityCategory Category { get; init; }

    /// <summary>
    /// The registry hive (e.g., "HKLM", "HKCU").
    /// </summary>
    public required string RegistryHive { get; init; }

    /// <summary>
    /// The registry key path (e.g., @"SOFTWARE\Microsoft\Windows Defender").
    /// </summary>
    public required string RegistryPath { get; init; }

    /// <summary>
    /// The registry value name.
    /// </summary>
    public required string ValueName { get; init; }

    /// <summary>
    /// The type of registry value.
    /// </summary>
    public required SettingValueType ValueType { get; init; }

    /// <summary>
    /// The value that enables (hardens) this security setting.
    /// </summary>
    public required object EnabledValue { get; init; }

    /// <summary>
    /// The value that disables (unhardened/default) this security setting.
    /// </summary>
    public required object DisabledValue { get; init; }

    /// <summary>
    /// The recommended value for this setting (typically same as EnabledValue).
    /// </summary>
    public required object RecommendedValue { get; init; }

    /// <summary>
    /// Compatibility / usability impact of enabling this setting.
    /// Defaults to <see cref="ImpactLevel.Unknown"/>; either set explicitly on the
    /// definition or filled in centrally from <c>SettingConsequencesCatalog</c>.
    /// </summary>
    public ImpactLevel Impact { get; set; } = ImpactLevel.Unknown;

    /// <summary>
    /// Short human-readable note describing what may break or change when this
    /// setting is enabled (compatibility impact, UX changes, log volume, etc.).
    /// Mirrors the per-setting note in <c>docs/security-setting-consequences.md</c>.
    /// </summary>
    public string Consequences { get; set; } = string.Empty;
}
