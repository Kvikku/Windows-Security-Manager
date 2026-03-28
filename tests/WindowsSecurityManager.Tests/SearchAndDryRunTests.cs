using Moq;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

public class SearchAndDryRunTests
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;

    public SearchAndDryRunTests()
    {
        _mockRegistry = new Mock<IRegistryService>();
        var provider = new TestSettingProvider();
        _manager = new SecuritySettingsManager(_mockRegistry.Object, new[] { provider });
    }

    // --- Search Tests ---

    [Fact]
    public void SearchSettings_ByName_ReturnsMatches()
    {
        var results = _manager.SearchSettings("Firewall");
        Assert.Single(results);
        Assert.Equal("TEST-003", results[0].Id);
    }

    [Fact]
    public void SearchSettings_ById_ReturnsMatches()
    {
        var results = _manager.SearchSettings("TEST-001");
        Assert.Single(results);
        Assert.Equal("TEST-001", results[0].Id);
    }

    [Fact]
    public void SearchSettings_ByDescription_ReturnsMatches()
    {
        var results = _manager.SearchSettings("First test");
        Assert.Single(results);
        Assert.Equal("TEST-001", results[0].Id);
    }

    [Fact]
    public void SearchSettings_CaseInsensitive_ReturnsMatches()
    {
        var results = _manager.SearchSettings("firewall");
        Assert.Single(results);
    }

    [Fact]
    public void SearchSettings_NoMatch_ReturnsEmpty()
    {
        var results = _manager.SearchSettings("nonexistent");
        Assert.Empty(results);
    }

    [Fact]
    public void SearchSettings_EmptyKeyword_ReturnsAll()
    {
        var results = _manager.SearchSettings("");
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SearchSettings_NullKeyword_ReturnsAll()
    {
        var results = _manager.SearchSettings(null!);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SearchSettings_WhitespaceKeyword_ReturnsAll()
    {
        var results = _manager.SearchSettings("   ");
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SearchSettings_PartialMatch_ReturnsMultiple()
    {
        // "Test Setting" matches "Test Setting 1", "Test Setting 2", and "Test Firewall Setting"
        var results = _manager.SearchSettings("Test Setting");
        Assert.Equal(3, results.Count);
    }

    // --- Dry Run Tests ---

    [Fact]
    public void DryRunEnable_ReturnsPlannedChanges()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(0);

        var settings = _manager.GetSettings(SecurityCategory.WindowsDefender);
        var changes = _manager.DryRunEnable(settings);

        Assert.Equal(2, changes.Count);
        Assert.Equal(1, changes[0].NewValue);
    }

    [Fact]
    public void DryRunDisable_ReturnsPlannedChanges()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);

        var settings = _manager.GetSettings(SecurityCategory.WindowsDefender);
        var changes = _manager.DryRunDisable(settings);

        Assert.Equal(2, changes.Count);
        Assert.Equal(0, changes[0].NewValue);
    }

    [Fact]
    public void DryRunEnable_DoesNotCallSetValue()
    {
        var settings = _manager.GetSettings();
        _manager.DryRunEnable(settings);

        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Never);
    }

    [Fact]
    public void DryRunDisable_DoesNotCallSetValue()
    {
        var settings = _manager.GetSettings();
        _manager.DryRunDisable(settings);

        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Never);
    }

    [Fact]
    public void DryRunEnable_ShowsCurrentValue()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(0);

        var settings = new[] { _manager.GetSettings().First() };
        var changes = _manager.DryRunEnable(settings);

        Assert.Equal(0, changes[0].CurrentValue);
        Assert.True(changes[0].IsCurrentlyConfigured);
    }

    [Fact]
    public void DryRunEnable_ShowsUnconfigured()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns((object?)null);

        var settings = new[] { _manager.GetSettings().First() };
        var changes = _manager.DryRunEnable(settings);

        Assert.Null(changes[0].CurrentValue);
        Assert.False(changes[0].IsCurrentlyConfigured);
    }

    // --- EnableSettings / DisableSettings (batch) Tests ---

    [Fact]
    public void EnableSettings_MultipleIds_EnablesAll()
    {
        int count = _manager.EnableSettings(new[] { "TEST-001", "TEST-002" });

        Assert.Equal(2, count);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(2));
    }

    [Fact]
    public void DisableSettings_MultipleIds_DisablesAll()
    {
        int count = _manager.DisableSettings(new[] { "TEST-001", "TEST-003" });

        Assert.Equal(2, count);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(2));
    }

    [Fact]
    public void EnableSettings_SomeInvalid_OnlyEnablesValid()
    {
        int count = _manager.EnableSettings(new[] { "TEST-001", "INVALID-999" });
        Assert.Equal(1, count);
    }

    [Fact]
    public void DisableSettings_EmptyList_ReturnsZero()
    {
        int count = _manager.DisableSettings(Array.Empty<string>());
        Assert.Equal(0, count);
    }
}
