namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents a curated security profile (preset) that enables a specific set of settings.
/// </summary>
public class SecurityProfile
{
    /// <summary>
    /// Unique name of the profile (e.g., "CIS Level 1").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of the profile and its intended use case.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The setting IDs that should be enabled when this profile is applied.
    /// </summary>
    public required IReadOnlyList<string> SettingIds { get; init; }
}
