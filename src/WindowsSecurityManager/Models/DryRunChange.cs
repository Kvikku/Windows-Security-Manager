namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents a planned registry change in dry-run mode.
/// </summary>
public class DryRunChange
{
    /// <summary>
    /// The setting that would be changed.
    /// </summary>
    public required SecuritySetting Setting { get; init; }

    /// <summary>
    /// The current registry value (null if not configured).
    /// </summary>
    public object? CurrentValue { get; init; }

    /// <summary>
    /// The value that would be written.
    /// </summary>
    public required object NewValue { get; init; }

    /// <summary>
    /// Whether the value currently exists in the registry.
    /// </summary>
    public bool IsCurrentlyConfigured { get; init; }
}
