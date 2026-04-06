namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents a backup of security setting registry values.
/// </summary>
public class BackupData
{
    /// <summary>
    /// Timestamp when the backup was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The backed-up setting entries.
    /// </summary>
    public required IReadOnlyList<BackupEntry> Entries { get; init; }
}

/// <summary>
/// A single backed-up setting value.
/// </summary>
public class BackupEntry
{
    /// <summary>
    /// The setting ID.
    /// </summary>
    public required string SettingId { get; init; }

    /// <summary>
    /// The registry hive.
    /// </summary>
    public required string RegistryHive { get; init; }

    /// <summary>
    /// The registry path.
    /// </summary>
    public required string RegistryPath { get; init; }

    /// <summary>
    /// The registry value name.
    /// </summary>
    public required string ValueName { get; init; }

    /// <summary>
    /// The value type.
    /// </summary>
    public required SettingValueType ValueType { get; init; }

    /// <summary>
    /// The value at the time of backup (null if not configured).
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Whether the value existed in the registry at backup time.
    /// </summary>
    public bool WasConfigured { get; init; }
}
