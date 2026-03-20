namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents the status of a single security setting.
/// </summary>
public class SettingStatus
{
    /// <summary>
    /// The security setting being reported on.
    /// </summary>
    public required SecuritySetting Setting { get; init; }

    /// <summary>
    /// The current value in the registry (null if not set).
    /// </summary>
    public object? CurrentValue { get; init; }

    /// <summary>
    /// Whether the setting is currently configured to its enabled (hardened) value.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// Whether the registry value exists.
    /// </summary>
    public required bool IsConfigured { get; init; }

    /// <summary>
    /// Whether the current value matches the recommended value.
    /// </summary>
    public required bool MatchesRecommended { get; init; }
}
