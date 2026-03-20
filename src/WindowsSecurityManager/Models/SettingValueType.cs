namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents the type of registry value for a security setting.
/// </summary>
public enum SettingValueType
{
    DWord,
    QWord,
    String,
    Binary,
    MultiString,
    ExpandString
}
