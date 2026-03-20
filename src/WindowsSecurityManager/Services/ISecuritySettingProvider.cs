using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Interface for providing security setting definitions.
/// Implement this interface to add new categories of security settings.
/// </summary>
public interface ISecuritySettingProvider
{
    /// <summary>
    /// Returns the collection of security settings defined by this provider.
    /// </summary>
    IEnumerable<SecuritySetting> GetSettings();
}
