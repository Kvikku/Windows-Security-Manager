using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Windows Firewall security settings for all profiles.
/// </summary>
public class FirewallSettings : ISecuritySettingProvider
{
    private const string Hive = "HKLM";
    private const string BasePath = @"SOFTWARE\Policies\Microsoft\WindowsFirewall";
    private const string DomainPath = BasePath + @"\DomainProfile";
    private const string PrivatePath = BasePath + @"\PrivateProfile";
    private const string PublicPath = BasePath + @"\PublicProfile";

    public IEnumerable<SecuritySetting> GetSettings()
    {
        // Domain Profile
        yield return new SecuritySetting
        {
            Id = "FW-001",
            Name = "Enable Firewall - Domain Profile",
            Description = "Enables Windows Firewall for the domain network profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = DomainPath,
            ValueName = "EnableFirewall",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-002",
            Name = "Default Inbound Action - Domain Profile",
            Description = "Sets default inbound action to Block for domain profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = DomainPath,
            ValueName = "DefaultInboundAction",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-003",
            Name = "Default Outbound Action - Domain Profile",
            Description = "Sets default outbound action to Allow for domain profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = DomainPath,
            ValueName = "DefaultOutboundAction",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "FW-004",
            Name = "Disable Notifications - Domain Profile",
            Description = "Disables firewall notification popups for domain profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = DomainPath,
            ValueName = "DisableNotifications",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-005",
            Name = "Enable Log Dropped Packets - Domain Profile",
            Description = "Enables logging of dropped packets for the domain firewall profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = DomainPath + @"\Logging",
            ValueName = "LogDroppedPackets",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-006",
            Name = "Enable Log Successful Connections - Domain Profile",
            Description = "Enables logging of successful connections for the domain firewall profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = DomainPath + @"\Logging",
            ValueName = "LogSuccessfulConnections",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        // Private Profile
        yield return new SecuritySetting
        {
            Id = "FW-007",
            Name = "Enable Firewall - Private Profile",
            Description = "Enables Windows Firewall for the private network profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PrivatePath,
            ValueName = "EnableFirewall",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-008",
            Name = "Default Inbound Action - Private Profile",
            Description = "Sets default inbound action to Block for private profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PrivatePath,
            ValueName = "DefaultInboundAction",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-009",
            Name = "Default Outbound Action - Private Profile",
            Description = "Sets default outbound action to Allow for private profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PrivatePath,
            ValueName = "DefaultOutboundAction",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "FW-010",
            Name = "Disable Notifications - Private Profile",
            Description = "Disables firewall notification popups for private profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PrivatePath,
            ValueName = "DisableNotifications",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-011",
            Name = "Enable Log Dropped Packets - Private Profile",
            Description = "Enables logging of dropped packets for the private firewall profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PrivatePath + @"\Logging",
            ValueName = "LogDroppedPackets",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-012",
            Name = "Enable Log Successful Connections - Private Profile",
            Description = "Enables logging of successful connections for the private firewall profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PrivatePath + @"\Logging",
            ValueName = "LogSuccessfulConnections",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        // Public Profile
        yield return new SecuritySetting
        {
            Id = "FW-013",
            Name = "Enable Firewall - Public Profile",
            Description = "Enables Windows Firewall for the public network profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PublicPath,
            ValueName = "EnableFirewall",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-014",
            Name = "Default Inbound Action - Public Profile",
            Description = "Sets default inbound action to Block for public profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PublicPath,
            ValueName = "DefaultInboundAction",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-015",
            Name = "Default Outbound Action - Public Profile",
            Description = "Sets default outbound action to Allow for public profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PublicPath,
            ValueName = "DefaultOutboundAction",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "FW-016",
            Name = "Disable Notifications - Public Profile",
            Description = "Disables firewall notification popups for public profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PublicPath,
            ValueName = "DisableNotifications",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-017",
            Name = "Enable Log Dropped Packets - Public Profile",
            Description = "Enables logging of dropped packets for the public firewall profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PublicPath + @"\Logging",
            ValueName = "LogDroppedPackets",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "FW-018",
            Name = "Enable Log Successful Connections - Public Profile",
            Description = "Enables logging of successful connections for the public firewall profile.",
            Category = SecurityCategory.Firewall,
            RegistryHive = Hive,
            RegistryPath = PublicPath + @"\Logging",
            ValueName = "LogSuccessfulConnections",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}
