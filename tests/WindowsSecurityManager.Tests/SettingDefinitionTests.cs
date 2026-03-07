using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Tests;

public class SettingDefinitionTests
{
    [Fact]
    public void DefenderSettings_ReturnsSettings()
    {
        var provider = new DefenderSettings();
        var settings = provider.GetSettings().ToList();

        Assert.NotEmpty(settings);
        Assert.All(settings, s => Assert.Equal(SecurityCategory.WindowsDefender, s.Category));
        Assert.All(settings, s => Assert.NotEmpty(s.Id));
        Assert.All(settings, s => Assert.NotEmpty(s.Name));
        Assert.All(settings, s => Assert.NotEmpty(s.Description));
        Assert.All(settings, s => Assert.Equal("HKLM", s.RegistryHive));
    }

    [Fact]
    public void AsrSettings_ReturnsSettings()
    {
        var provider = new AsrSettings();
        var settings = provider.GetSettings().ToList();

        Assert.NotEmpty(settings);
        Assert.All(settings, s => Assert.Equal(SecurityCategory.AttackSurfaceReduction, s.Category));
        Assert.All(settings, s => Assert.NotEmpty(s.Id));
        Assert.All(settings, s => Assert.Equal(SettingValueType.DWord, s.ValueType));
        // All ASR rules should have enabled=1 (Block) and disabled=0
        Assert.All(settings, s => Assert.Equal(1, s.EnabledValue));
        Assert.All(settings, s => Assert.Equal(0, s.DisabledValue));
    }

    [Fact]
    public void FirewallSettings_ReturnsSettings()
    {
        var provider = new FirewallSettings();
        var settings = provider.GetSettings().ToList();

        Assert.NotEmpty(settings);
        Assert.All(settings, s => Assert.Equal(SecurityCategory.Firewall, s.Category));
        Assert.All(settings, s => Assert.NotEmpty(s.Id));
    }

    [Fact]
    public void CisBenchmarkSettings_ReturnsSettings()
    {
        var provider = new CisBenchmarkSettings();
        var settings = provider.GetSettings().ToList();

        Assert.NotEmpty(settings);
        Assert.All(settings, s => Assert.Equal(SecurityCategory.CisBenchmark, s.Category));
        Assert.All(settings, s => Assert.NotEmpty(s.Id));
    }

    [Fact]
    public void AccountPolicySettings_ReturnsSettings()
    {
        var provider = new AccountPolicySettings();
        var settings = provider.GetSettings().ToList();

        Assert.NotEmpty(settings);
        Assert.All(settings, s => Assert.Equal(SecurityCategory.AccountPolicy, s.Category));
        Assert.All(settings, s => Assert.NotEmpty(s.Id));
    }

    [Fact]
    public void NetworkSecuritySettings_ReturnsSettings()
    {
        var provider = new NetworkSecuritySettings();
        var settings = provider.GetSettings().ToList();

        Assert.NotEmpty(settings);
        Assert.All(settings, s => Assert.Equal(SecurityCategory.NetworkSecurity, s.Category));
        Assert.All(settings, s => Assert.NotEmpty(s.Id));
    }

    [Fact]
    public void AllProviders_HaveUniqueIds()
    {
        var allSettings = new List<SecuritySetting>();
        allSettings.AddRange(new DefenderSettings().GetSettings());
        allSettings.AddRange(new AsrSettings().GetSettings());
        allSettings.AddRange(new FirewallSettings().GetSettings());
        allSettings.AddRange(new CisBenchmarkSettings().GetSettings());
        allSettings.AddRange(new AccountPolicySettings().GetSettings());
        allSettings.AddRange(new NetworkSecuritySettings().GetSettings());

        var ids = allSettings.Select(s => s.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AllProviders_HaveValidRegistryHives()
    {
        var validHives = new[] { "HKLM", "HKCU", "HKCR", "HKU", "HKCC" };
        var allSettings = new List<SecuritySetting>();
        allSettings.AddRange(new DefenderSettings().GetSettings());
        allSettings.AddRange(new AsrSettings().GetSettings());
        allSettings.AddRange(new FirewallSettings().GetSettings());
        allSettings.AddRange(new CisBenchmarkSettings().GetSettings());
        allSettings.AddRange(new AccountPolicySettings().GetSettings());
        allSettings.AddRange(new NetworkSecuritySettings().GetSettings());

        Assert.All(allSettings, s => Assert.Contains(s.RegistryHive, validHives));
    }

    [Fact]
    public void AllProviders_HaveNonNullValues()
    {
        var allSettings = new List<SecuritySetting>();
        allSettings.AddRange(new DefenderSettings().GetSettings());
        allSettings.AddRange(new AsrSettings().GetSettings());
        allSettings.AddRange(new FirewallSettings().GetSettings());
        allSettings.AddRange(new CisBenchmarkSettings().GetSettings());
        allSettings.AddRange(new AccountPolicySettings().GetSettings());
        allSettings.AddRange(new NetworkSecuritySettings().GetSettings());

        Assert.All(allSettings, s =>
        {
            Assert.NotNull(s.EnabledValue);
            Assert.NotNull(s.DisabledValue);
            Assert.NotNull(s.RecommendedValue);
            Assert.NotEmpty(s.RegistryPath);
            Assert.NotEmpty(s.ValueName);
        });
    }
}
