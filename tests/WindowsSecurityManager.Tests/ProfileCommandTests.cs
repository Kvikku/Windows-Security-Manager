using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class ProfileCommandTests : IDisposable
{
    private readonly CommandTestContext _context;

    public ProfileCommandTests()
    {
        _context = new CommandTestContext();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Profile_List_ShowsAllProfiles()
    {
        var cmd = ProfileCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--list" });

        var output = _context.GetOutput();
        Assert.Contains("CIS Level 1", output);
        Assert.Contains("Maximum Security", output);
        Assert.Contains("Developer Workstation", output);
        Assert.Contains("Security Profiles", output);
    }

    [Fact]
    public async Task Profile_NoArgs_ShowsProfiles()
    {
        var cmd = ProfileCommand.Create(_context.Manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = _context.GetOutput();
        Assert.Contains("CIS Level 1", output);
        Assert.Contains("Maximum Security", output);
        Assert.Contains("Developer Workstation", output);
    }

    [Fact]
    public async Task Profile_Apply_ValidProfile_EnablesSettings()
    {
        var cmd = ProfileCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--apply", "CIS Level 1" });

        var output = _context.GetOutput();
        Assert.Contains("Applied profile 'CIS Level 1'", output);
    }

    [Fact]
    public async Task Profile_Apply_InvalidProfile_ShowsNotFound()
    {
        var cmd = ProfileCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--apply", "Nonexistent Profile" });

        var output = _context.GetOutput();
        Assert.Contains("Profile 'Nonexistent Profile' not found.", output);
        Assert.Contains("--list", output);
    }

    [Fact]
    public async Task Profile_Apply_DryRun_ShowsPreview()
    {
        var cmd = ProfileCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--apply", "CIS Level 1", "--dry-run" });

        var output = _context.GetOutput();
        Assert.Contains("DRY RUN", output);
        Assert.Contains("CIS Level 1", output);
        Assert.Contains("Total:", output);
        Assert.Contains("settings would be changed.", output);
    }
}
