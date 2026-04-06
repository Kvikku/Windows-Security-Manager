using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Network security hardening settings.
/// </summary>
public class NetworkSecuritySettings : ISecuritySettingProvider
{
    private const string Hive = "HKLM";

    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return new SecuritySetting
        {
            Id = "NET-001",
            Name = "Disable LLMNR",
            Description = "Disables Link-Local Multicast Name Resolution to prevent MITM attacks.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows NT\DNSClient",
            ValueName = "EnableMulticast",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-002",
            Name = "Disable NetBIOS over TCP/IP",
            Description = "Disables NetBIOS name resolution to prevent name spoofing attacks.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\NetBT\Parameters",
            ValueName = "NodeType",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 0,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "NET-003",
            Name = "Disable WPAD",
            Description = "Disables Web Proxy Auto-Discovery protocol.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Wpad",
            ValueName = "WpadOverride",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "NET-004",
            Name = "Enable DNS over HTTPS",
            Description = "Enables DNS-over-HTTPS for enhanced privacy.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters",
            ValueName = "EnableAutoDoh",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 0,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "NET-005",
            Name = "Disable WINS Resolution",
            Description = "Disables WINS name resolution to reduce attack surface.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\NetBT\Parameters",
            ValueName = "EnableLMHOSTS",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-006",
            Name = "Configure TLS 1.2 (Client)",
            Description = "Enables TLS 1.2 for client connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "NET-007",
            Name = "Configure TLS 1.2 (Server)",
            Description = "Enables TLS 1.2 for server connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "NET-008",
            Name = "Disable SSL 2.0 (Client)",
            Description = "Disables insecure SSL 2.0 protocol for client connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 2.0\Client",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-009",
            Name = "Disable SSL 2.0 (Server)",
            Description = "Disables insecure SSL 2.0 protocol for server connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 2.0\Server",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-010",
            Name = "Disable SSL 3.0 (Client)",
            Description = "Disables insecure SSL 3.0 protocol for client connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 3.0\Client",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-011",
            Name = "Disable SSL 3.0 (Server)",
            Description = "Disables insecure SSL 3.0 protocol for server connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 3.0\Server",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-012",
            Name = "Disable TLS 1.0 (Client)",
            Description = "Disables legacy TLS 1.0 protocol for client connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Client",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-013",
            Name = "Disable TLS 1.0 (Server)",
            Description = "Disables legacy TLS 1.0 protocol for server connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Server",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-014",
            Name = "Disable TLS 1.1 (Client)",
            Description = "Disables legacy TLS 1.1 protocol for client connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Client",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "NET-015",
            Name = "Disable TLS 1.1 (Server)",
            Description = "Disables legacy TLS 1.1 protocol for server connections.",
            Category = SecurityCategory.NetworkSecurity,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Server",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };
    }
}
