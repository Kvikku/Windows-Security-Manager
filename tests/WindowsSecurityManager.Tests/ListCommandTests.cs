using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class ListCommandTests : IDisposable
{
    private readonly CommandTestContext _context;

    public ListCommandTests()
    {
        _context = new CommandTestContext();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task List_NoFilters_ShowsAllSettings()
    {
        var cmd = ListCommand.Create(_context.Manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = _context.GetOutput();
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.Contains("TEST-003", output);
        Assert.Contains("Total: 3 settings", output);
    }

    [Fact]
    public async Task List_WithCategory_FiltersSettings()
    {
        var cmd = ListCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--category", "WindowsDefender" });

        var output = _context.GetOutput();
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.DoesNotContain("TEST-003", output);
        Assert.Contains("Total: 2 settings", output);
    }

    [Fact]
    public async Task List_WithSearch_SearchesSettings()
    {
        var cmd = ListCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--search", "Firewall" });

        var output = _context.GetOutput();
        Assert.Contains("TEST-003", output);
        Assert.Contains("Search: \"Firewall\"", output);
        Assert.Contains("Total: 1 settings", output);
    }

    [Fact]
    public async Task List_WithSearchAndCategory_AppliesBothFilters()
    {
        var cmd = ListCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--search", "Test", "--category", "WindowsDefender" });

        var output = _context.GetOutput();
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.DoesNotContain("TEST-003", output);
    }
}
