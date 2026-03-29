using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class ProfileCommandTests : IDisposable
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly TextWriter _originalOut;
    private readonly StringWriter _writer;

    public ProfileCommandTests()
    {
        _mockRegistry = new Mock<IRegistryService>();
        _manager = new SecuritySettingsManager(_mockRegistry.Object, new[] { new TestSettingProvider() });
        _originalOut = Console.Out;
        _writer = new StringWriter();
        Console.SetOut(_writer);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOut);
        _writer.Dispose();
    }

    private string GetOutput() => _writer.ToString();

    [Fact]
    public async Task Profile_List_ShowsAllProfiles()
    {
        var cmd = ProfileCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--list" });

        var output = GetOutput();
        Assert.Contains("CIS Level 1", output);
        Assert.Contains("Maximum Security", output);
        Assert.Contains("Developer Workstation", output);
        Assert.Contains("Security Profiles", output);
    }

    [Fact]
    public async Task Profile_NoArgs_ShowsProfiles()
    {
        var cmd = ProfileCommand.Create(_manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = GetOutput();
        Assert.Contains("CIS Level 1", output);
        Assert.Contains("Maximum Security", output);
        Assert.Contains("Developer Workstation", output);
    }

    [Fact]
    public async Task Profile_Apply_ValidProfile_EnablesSettings()
    {
        var cmd = ProfileCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--apply", "CIS Level 1" });

        var output = GetOutput();
        Assert.Contains("Applied profile 'CIS Level 1'", output);
    }

    [Fact]
    public async Task Profile_Apply_InvalidProfile_ShowsNotFound()
    {
        var cmd = ProfileCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--apply", "Nonexistent Profile" });

        var output = GetOutput();
        Assert.Contains("Profile 'Nonexistent Profile' not found.", output);
        Assert.Contains("--list", output);
    }

    [Fact]
    public async Task Profile_Apply_DryRun_ShowsPreview()
    {
        var cmd = ProfileCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--apply", "CIS Level 1", "--dry-run" });

        var output = GetOutput();
        Assert.Contains("DRY RUN", output);
        Assert.Contains("CIS Level 1", output);
        Assert.Contains("Total:", output);
        Assert.Contains("settings would be changed.", output);
    }
}
