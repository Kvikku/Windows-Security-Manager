using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Account policy and network security settings.
/// </summary>
public class AccountPolicySettings : ISecuritySettingProvider
{
    private const string Hive = "HKLM";

    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return new SecuritySetting
        {
            Id = "ACCT-001",
            Name = "Account Lockout Threshold",
            Description = "Sets account lockout threshold to 5 invalid logon attempts.",
            Category = SecurityCategory.AccountPolicy,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\RemoteAccess\Parameters\AccountLockout",
            ValueName = "MaxDenials",
            ValueType = SettingValueType.DWord,
            EnabledValue = 5,
            DisabledValue = 0,
            RecommendedValue = 5
        };

        yield return new SecuritySetting
        {
            Id = "ACCT-002",
            Name = "Account Lockout Duration",
            Description = "Sets account lockout duration to 30 minutes.",
            Category = SecurityCategory.AccountPolicy,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\RemoteAccess\Parameters\AccountLockout",
            ValueName = "ResetTime",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1800,
            DisabledValue = 0,
            RecommendedValue = 1800
        };

        yield return new SecuritySetting
        {
            Id = "ACCT-003",
            Name = "Disable Guest Account",
            Description = "Ensures the guest account is disabled.",
            Category = SecurityCategory.AccountPolicy,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon",
            ValueName = "AllowGuest",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "ACCT-004",
            Name = "Enable Audit Policy Subcategory Override",
            Description = "Forces audit policy subcategory settings to override audit policy category settings.",
            Category = SecurityCategory.AccountPolicy,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "SCENoApplyLegacyAuditPolicy",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "ACCT-005",
            Name = "Enable Crash On Audit Fail",
            Description = "System halts when unable to log security events.",
            Category = SecurityCategory.AccountPolicy,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "CrashOnAuditFail",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}
