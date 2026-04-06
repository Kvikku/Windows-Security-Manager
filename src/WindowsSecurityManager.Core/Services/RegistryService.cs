using System.Runtime.Versioning;
using Microsoft.Win32;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Windows Registry service implementation using Microsoft.Win32.Registry.
/// </summary>
[SupportedOSPlatform("windows")]
public class RegistryService : IRegistryService
{
    public object? GetValue(string hive, string path, string valueName)
    {
        using var key = OpenBaseKey(hive).OpenSubKey(path);
        return key?.GetValue(valueName);
    }

    public void SetValue(string hive, string path, string valueName, object value, SettingValueType valueType)
    {
        using var key = OpenBaseKey(hive).CreateSubKey(path, true);
        key.SetValue(valueName, value, ToRegistryValueKind(valueType));
    }

    public bool ValueExists(string hive, string path, string valueName)
    {
        using var key = OpenBaseKey(hive).OpenSubKey(path);
        return key?.GetValue(valueName) != null;
    }

    public void DeleteValue(string hive, string path, string valueName)
    {
        using var key = OpenBaseKey(hive).OpenSubKey(path, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    private static RegistryKey OpenBaseKey(string hive)
    {
        return hive.ToUpperInvariant() switch
        {
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKCR" or "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
            "HKU" or "HKEY_USERS" => Registry.Users,
            "HKCC" or "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
            _ => throw new ArgumentException($"Unknown registry hive: {hive}")
        };
    }

    private static RegistryValueKind ToRegistryValueKind(SettingValueType valueType)
    {
        return valueType switch
        {
            SettingValueType.DWord => RegistryValueKind.DWord,
            SettingValueType.QWord => RegistryValueKind.QWord,
            SettingValueType.String => RegistryValueKind.String,
            SettingValueType.Binary => RegistryValueKind.Binary,
            SettingValueType.MultiString => RegistryValueKind.MultiString,
            SettingValueType.ExpandString => RegistryValueKind.ExpandString,
            _ => throw new ArgumentException($"Unknown value type: {valueType}")
        };
    }
}
