using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class EnableCommandTests : IDisposable
{
    private readonly CommandTestContext _context;

    public EnableCommandTests()
    {
        _context = new CommandTestContext();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Enable_WithSetting_EnablesSetting()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--setting", "TEST-001" });

        var output = _context.GetOutput();
        Assert.Contains("Setting 'TEST-001' enabled successfully.", output);
        _context.MockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue1", 1, SettingValueType.DWord), Times.Once);
    }

    [Fact]
    public async Task Enable_WithInvalidSetting_ShowsNotFound()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--setting", "INVALID-999" });

        var output = _context.GetOutput();
        Assert.Contains("Setting 'INVALID-999' not found.", output);
        _context.MockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Never);
    }

    [Fact]
    public async Task Enable_WithCategory_EnablesCategorySettings()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--category", "WindowsDefender" });

        var output = _context.GetOutput();
        Assert.Contains("Enabled 2 settings in category 'WindowsDefender'.", output);
    }

    [Fact]
    public async Task Enable_WithAll_EnablesAllSettings()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--all" });

        var output = _context.GetOutput();
        Assert.Contains("Enabled 3 security settings.", output);
        _context.MockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Enable_NoOption_ShowsUsageMessage()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = _context.GetOutput();
        Assert.Contains("Please specify --setting, --category, or --all.", output);
    }

    [Fact]
    public async Task Enable_WithAllDryRun_ShowsPreview()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--all", "--dry-run" });

        var output = _context.GetOutput();
        Assert.Contains("DRY RUN", output);
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.Contains("TEST-003", output);
        Assert.Contains("Total: 3 settings would be changed.", output);
    }

    [Fact]
    public async Task Enable_WithSettingDryRun_ShowsPreview()
    {
        var cmd = EnableCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--setting", "TEST-001", "--dry-run" });

        var output = _context.GetOutput();
        Assert.Contains("DRY RUN", output);
        Assert.Contains("TEST-001", output);
        Assert.Contains("Total: 1 settings would be changed.", output);
    }
}
