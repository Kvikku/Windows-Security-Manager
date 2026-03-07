using Moq;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

public class SecuritySettingsManagerTests
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly TestSettingProvider _testProvider;

    public SecuritySettingsManagerTests()
    {
        _mockRegistry = new Mock<IRegistryService>();
        _testProvider = new TestSettingProvider();
        _manager = new SecuritySettingsManager(_mockRegistry.Object, new[] { _testProvider });
    }

    [Fact]
    public void GetSettings_ReturnsAllSettings()
    {
        var settings = _manager.GetSettings();
        Assert.Equal(3, settings.Count);
    }

    [Fact]
    public void GetSettings_FiltersByCategory()
    {
        var defenderSettings = _manager.GetSettings(SecurityCategory.WindowsDefender);
        Assert.Equal(2, defenderSettings.Count);
        Assert.All(defenderSettings, s => Assert.Equal(SecurityCategory.WindowsDefender, s.Category));

        var firewallSettings = _manager.GetSettings(SecurityCategory.Firewall);
        Assert.Single(firewallSettings);
    }

    [Fact]
    public void GetSettings_ReturnsEmptyForUnusedCategory()
    {
        var settings = _manager.GetSettings(SecurityCategory.AttackSurfaceReduction);
        Assert.Empty(settings);
    }

    [Fact]
    public void EnableSetting_ValidId_SetsEnabledValue()
    {
        bool result = _manager.EnableSetting("TEST-001");

        Assert.True(result);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue1", 1, SettingValueType.DWord), Times.Once);
    }

    [Fact]
    public void EnableSetting_InvalidId_ReturnsFalse()
    {
        bool result = _manager.EnableSetting("INVALID-999");
        Assert.False(result);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Never);
    }

    [Fact]
    public void EnableSetting_IsCaseInsensitive()
    {
        bool result = _manager.EnableSetting("test-001");
        Assert.True(result);
    }

    [Fact]
    public void DisableSetting_ValidId_SetsDisabledValue()
    {
        bool result = _manager.DisableSetting("TEST-001");

        Assert.True(result);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue1", 0, SettingValueType.DWord), Times.Once);
    }

    [Fact]
    public void DisableSetting_InvalidId_ReturnsFalse()
    {
        bool result = _manager.DisableSetting("INVALID-999");
        Assert.False(result);
    }

    [Fact]
    public void EnableCategory_EnablesAllSettingsInCategory()
    {
        int count = _manager.EnableCategory(SecurityCategory.WindowsDefender);

        Assert.Equal(2, count);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue1", 1, SettingValueType.DWord), Times.Once);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue2", 1, SettingValueType.DWord), Times.Once);
    }

    [Fact]
    public void DisableCategory_DisablesAllSettingsInCategory()
    {
        int count = _manager.DisableCategory(SecurityCategory.WindowsDefender);

        Assert.Equal(2, count);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue1", 0, SettingValueType.DWord), Times.Once);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue2", 0, SettingValueType.DWord), Times.Once);
    }

    [Fact]
    public void EnableAll_EnablesAllSettings()
    {
        int count = _manager.EnableAll();

        Assert.Equal(3, count);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(3));
    }

    [Fact]
    public void DisableAll_DisablesAllSettings()
    {
        int count = _manager.DisableAll();

        Assert.Equal(3, count);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(3));
    }

    [Fact]
    public void GenerateReport_AllEnabled_ShowsCorrectStatus()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "FwValue1")).Returns(1);

        var report = _manager.GenerateReport();

        Assert.Equal(3, report.TotalSettings);
        Assert.Equal(3, report.EnabledCount);
        Assert.Equal(0, report.DisabledCount);
        Assert.Equal(0, report.NotConfiguredCount);
        Assert.Equal(100.0, report.CompliancePercentage);
    }

    [Fact]
    public void GenerateReport_NoneConfigured_ShowsCorrectStatus()
    {
        _mockRegistry.Setup(r => r.GetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns((object?)null);

        var report = _manager.GenerateReport();

        Assert.Equal(3, report.TotalSettings);
        Assert.Equal(0, report.EnabledCount);
        Assert.Equal(3, report.DisabledCount);
        Assert.Equal(3, report.NotConfiguredCount);
        Assert.Equal(0.0, report.CompliancePercentage);
    }

    [Fact]
    public void GenerateReport_MixedStatus_ShowsCorrectStatus()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(0);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "FwValue1")).Returns((object?)null);

        var report = _manager.GenerateReport();

        Assert.Equal(3, report.TotalSettings);
        Assert.Equal(1, report.EnabledCount);
        Assert.Equal(2, report.DisabledCount);
        Assert.Equal(1, report.NotConfiguredCount);
        Assert.Equal(33.3, report.CompliancePercentage);
    }

    [Fact]
    public void GenerateReport_FiltersByCategory()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(1);

        var report = _manager.GenerateReport(SecurityCategory.WindowsDefender);

        Assert.Equal(2, report.TotalSettings);
        Assert.Equal(2, report.EnabledCount);
        Assert.All(report.Settings, s =>
            Assert.Equal(SecurityCategory.WindowsDefender, s.Setting.Category));
    }

    [Fact]
    public void GetSettingStatus_EnabledSetting_ReportsCorrectly()
    {
        var setting = _testProvider.GetSettings().First();
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);

        var status = _manager.GetSettingStatus(setting);

        Assert.True(status.IsEnabled);
        Assert.True(status.IsConfigured);
        Assert.True(status.MatchesRecommended);
        Assert.Equal(1, status.CurrentValue);
    }

    [Fact]
    public void GetSettingStatus_DisabledSetting_ReportsCorrectly()
    {
        var setting = _testProvider.GetSettings().First();
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(0);

        var status = _manager.GetSettingStatus(setting);

        Assert.False(status.IsEnabled);
        Assert.True(status.IsConfigured);
        Assert.False(status.MatchesRecommended);
        Assert.Equal(0, status.CurrentValue);
    }

    [Fact]
    public void GetSettingStatus_MissingSetting_ReportsCorrectly()
    {
        var setting = _testProvider.GetSettings().First();
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns((object?)null);

        var status = _manager.GetSettingStatus(setting);

        Assert.False(status.IsEnabled);
        Assert.False(status.IsConfigured);
        Assert.False(status.MatchesRecommended);
        Assert.Null(status.CurrentValue);
    }

    [Fact]
    public void Constructor_NullRegistryService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SecuritySettingsManager(null!, new[] { _testProvider }));
    }

    [Fact]
    public void Constructor_NullProviders_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SecuritySettingsManager(_mockRegistry.Object, null!));
    }

    [Fact]
    public void MultipleProviders_AggregatesSettings()
    {
        var provider1 = new TestSettingProvider();
        var provider2 = new TestSettingProvider2();

        var manager = new SecuritySettingsManager(_mockRegistry.Object, new ISecuritySettingProvider[] { provider1, provider2 });
        var settings = manager.GetSettings();

        Assert.Equal(4, settings.Count);
    }
}

/// <summary>
/// Test setting provider with known settings for unit testing.
/// </summary>
internal class TestSettingProvider : ISecuritySettingProvider
{
    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return new SecuritySetting
        {
            Id = "TEST-001",
            Name = "Test Setting 1",
            Description = "First test setting",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = "HKLM",
            RegistryPath = @"SOFTWARE\Test",
            ValueName = "TestValue1",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "TEST-002",
            Name = "Test Setting 2",
            Description = "Second test setting",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = "HKLM",
            RegistryPath = @"SOFTWARE\Test",
            ValueName = "TestValue2",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "TEST-003",
            Name = "Test Firewall Setting",
            Description = "A firewall test setting",
            Category = SecurityCategory.Firewall,
            RegistryHive = "HKLM",
            RegistryPath = @"SOFTWARE\Test",
            ValueName = "FwValue1",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}

/// <summary>
/// Additional test provider to verify multiple provider aggregation.
/// </summary>
internal class TestSettingProvider2 : ISecuritySettingProvider
{
    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return new SecuritySetting
        {
            Id = "TEST2-001",
            Name = "Extra Test Setting",
            Description = "Extra test setting from second provider",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = "HKLM",
            RegistryPath = @"SOFTWARE\Test2",
            ValueName = "ExtraValue",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}
