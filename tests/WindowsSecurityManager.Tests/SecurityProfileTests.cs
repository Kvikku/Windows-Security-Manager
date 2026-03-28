using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Tests;

public class SecurityProfileTests
{
    [Fact]
    public void GetProfiles_ReturnsThreeProfiles()
    {
        var profiles = SecurityProfiles.GetProfiles();
        Assert.Equal(3, profiles.Count);
    }

    [Fact]
    public void AllProfiles_HaveNames()
    {
        var profiles = SecurityProfiles.GetProfiles();
        Assert.All(profiles, p => Assert.NotEmpty(p.Name));
    }

    [Fact]
    public void AllProfiles_HaveDescriptions()
    {
        var profiles = SecurityProfiles.GetProfiles();
        Assert.All(profiles, p => Assert.NotEmpty(p.Description));
    }

    [Fact]
    public void AllProfiles_HaveSettings()
    {
        var profiles = SecurityProfiles.GetProfiles();
        Assert.All(profiles, p => Assert.NotEmpty(p.SettingIds));
    }

    [Fact]
    public void CisLevel1_ContainsDefenderAndFirewall()
    {
        var profile = SecurityProfiles.CisLevel1();
        Assert.Contains(profile.SettingIds, id => id.StartsWith("DEF-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("FW-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("CIS-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("ACCT-"));
    }

    [Fact]
    public void MaximumSecurity_ContainsAllCategories()
    {
        var profile = SecurityProfiles.MaximumSecurity();
        Assert.Contains(profile.SettingIds, id => id.StartsWith("DEF-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("ASR-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("FW-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("CIS-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("ACCT-"));
        Assert.Contains(profile.SettingIds, id => id.StartsWith("NET-"));
    }

    [Fact]
    public void MaximumSecurity_HasMostSettings()
    {
        var profiles = SecurityProfiles.GetProfiles();
        var max = profiles.First(p => p.Name == "Maximum Security");
        Assert.True(profiles.All(p => p.SettingIds.Count <= max.SettingIds.Count));
    }

    [Fact]
    public void DeveloperWorkstation_HasFewerSettingsThanMax()
    {
        var dev = SecurityProfiles.DeveloperWorkstation();
        var max = SecurityProfiles.MaximumSecurity();
        Assert.True(dev.SettingIds.Count < max.SettingIds.Count);
    }

    [Fact]
    public void AllProfiles_ReferenceValidSettingIds()
    {
        // Collect all valid setting IDs from all providers
        var allSettings = new List<SecuritySetting>();
        allSettings.AddRange(new DefenderSettings().GetSettings());
        allSettings.AddRange(new AsrSettings().GetSettings());
        allSettings.AddRange(new FirewallSettings().GetSettings());
        allSettings.AddRange(new CisBenchmarkSettings().GetSettings());
        allSettings.AddRange(new AccountPolicySettings().GetSettings());
        allSettings.AddRange(new NetworkSecuritySettings().GetSettings());

        var validIds = allSettings.Select(s => s.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var profiles = SecurityProfiles.GetProfiles();
        foreach (var profile in profiles)
        {
            Assert.All(profile.SettingIds, id =>
                Assert.True(validIds.Contains(id), $"Profile '{profile.Name}' references unknown setting ID '{id}'"));
        }
    }
}
