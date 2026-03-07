using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Tests;

public class SecurityReportTests
{
    [Fact]
    public void CompliancePercentage_AllEnabled_Returns100()
    {
        var report = new SecurityReport
        {
            Settings = new List<SettingStatus>
            {
                CreateStatus(isEnabled: true, isConfigured: true, matchesRecommended: true),
                CreateStatus(isEnabled: true, isConfigured: true, matchesRecommended: true),
            }
        };

        Assert.Equal(100.0, report.CompliancePercentage);
    }

    [Fact]
    public void CompliancePercentage_NoneEnabled_Returns0()
    {
        var report = new SecurityReport
        {
            Settings = new List<SettingStatus>
            {
                CreateStatus(isEnabled: false, isConfigured: false, matchesRecommended: false),
                CreateStatus(isEnabled: false, isConfigured: true, matchesRecommended: false),
            }
        };

        Assert.Equal(0.0, report.CompliancePercentage);
    }

    [Fact]
    public void CompliancePercentage_HalfEnabled_Returns50()
    {
        var report = new SecurityReport
        {
            Settings = new List<SettingStatus>
            {
                CreateStatus(isEnabled: true, isConfigured: true, matchesRecommended: true),
                CreateStatus(isEnabled: false, isConfigured: true, matchesRecommended: false),
            }
        };

        Assert.Equal(50.0, report.CompliancePercentage);
    }

    [Fact]
    public void CompliancePercentage_EmptyReport_Returns0()
    {
        var report = new SecurityReport
        {
            Settings = new List<SettingStatus>()
        };

        Assert.Equal(0.0, report.CompliancePercentage);
    }

    [Fact]
    public void Counts_AreCorrect()
    {
        var report = new SecurityReport
        {
            Settings = new List<SettingStatus>
            {
                CreateStatus(isEnabled: true, isConfigured: true, matchesRecommended: true),
                CreateStatus(isEnabled: false, isConfigured: true, matchesRecommended: false),
                CreateStatus(isEnabled: false, isConfigured: false, matchesRecommended: false),
            }
        };

        Assert.Equal(3, report.TotalSettings);
        Assert.Equal(1, report.EnabledCount);
        Assert.Equal(2, report.DisabledCount);
        Assert.Equal(1, report.NotConfiguredCount);
        Assert.Equal(1, report.MatchesRecommendedCount);
    }

    private static SettingStatus CreateStatus(bool isEnabled, bool isConfigured, bool matchesRecommended)
    {
        return new SettingStatus
        {
            Setting = new SecuritySetting
            {
                Id = "TEST",
                Name = "Test",
                Description = "Test",
                Category = SecurityCategory.WindowsDefender,
                RegistryHive = "HKLM",
                RegistryPath = @"SOFTWARE\Test",
                ValueName = "Test",
                ValueType = SettingValueType.DWord,
                EnabledValue = 1,
                DisabledValue = 0,
                RecommendedValue = 1
            },
            CurrentValue = isConfigured ? (isEnabled ? 1 : 0) : null,
            IsEnabled = isEnabled,
            IsConfigured = isConfigured,
            MatchesRecommended = matchesRecommended
        };
    }
}
