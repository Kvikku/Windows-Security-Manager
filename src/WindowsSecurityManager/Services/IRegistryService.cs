using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Abstraction for Windows registry operations. Enables testing without actual registry access.
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// Gets a registry value. Returns null if the value does not exist.
    /// </summary>
    object? GetValue(string hive, string path, string valueName);

    /// <summary>
    /// Sets a registry value, creating the key if it does not exist.
    /// </summary>
    void SetValue(string hive, string path, string valueName, object value, SettingValueType valueType);

    /// <summary>
    /// Checks if a specific registry value exists.
    /// </summary>
    bool ValueExists(string hive, string path, string valueName);

    /// <summary>
    /// Deletes a registry value if it exists.
    /// </summary>
    void DeleteValue(string hive, string path, string valueName);
}
