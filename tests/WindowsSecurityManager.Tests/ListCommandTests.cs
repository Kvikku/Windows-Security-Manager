using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class ListCommandTests : IDisposable
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly TextWriter _originalOut;
    private readonly StringWriter _writer;

    public ListCommandTests()
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
    public async Task List_NoFilters_ShowsAllSettings()
    {
        var cmd = ListCommand.Create(_manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = GetOutput();
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.Contains("TEST-003", output);
        Assert.Contains("Total: 3 settings", output);
    }

    [Fact]
    public async Task List_WithCategory_FiltersSettings()
    {
        var cmd = ListCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--category", "WindowsDefender" });

        var output = GetOutput();
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.DoesNotContain("TEST-003", output);
        Assert.Contains("Total: 2 settings", output);
    }

    [Fact]
    public async Task List_WithSearch_SearchesSettings()
    {
        var cmd = ListCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--search", "Firewall" });

        var output = GetOutput();
        Assert.Contains("TEST-003", output);
        Assert.Contains("Search: \"Firewall\"", output);
        Assert.Contains("Total: 1 settings", output);
    }

    [Fact]
    public async Task List_WithSearchAndCategory_AppliesBothFilters()
    {
        var cmd = ListCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--search", "Test", "--category", "WindowsDefender" });

        var output = GetOutput();
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.DoesNotContain("TEST-003", output);
    }
}
