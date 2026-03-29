using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class DetailCommandTests : IDisposable
{
    private readonly CommandTestContext _context;

    public DetailCommandTests()
    {
        _context = new CommandTestContext();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Detail_ValidSetting_ShowsAllDetails()
    {
        var cmd = DetailCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "TEST-001" });

        var output = _context.GetOutput();
        Assert.Contains("ID:            TEST-001", output);
        Assert.Contains("Name:          Test Setting 1", output);
        Assert.Contains("Description:   First test setting", output);
        Assert.Contains("Category:      WindowsDefender", output);
        Assert.Contains("Hive:        HKLM", output);
        Assert.Contains(@"Path:        SOFTWARE\Test", output);
        Assert.Contains("Value Name:  TestValue1", output);
        Assert.Contains("Enabled:     1", output);
        Assert.Contains("Disabled:    0", output);
    }

    [Fact]
    public async Task Detail_InvalidSetting_ShowsNotFound()
    {
        var cmd = DetailCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "INVALID-999" });

        var output = _context.GetOutput();
        Assert.Contains("Setting 'INVALID-999' not found.", output);
    }

    [Fact]
    public async Task Detail_EnabledSetting_ShowsEnabledStatus()
    {
        _context.MockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        var cmd = DetailCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "TEST-001" });

        var output = _context.GetOutput();
        Assert.Contains("ENABLED", output);
        Assert.Contains("Current Value:  1", output);
        Assert.Contains("Is Configured:  True", output);
        Assert.Contains("Matches Recommended: True", output);
    }
}
