using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class DisableCommandTests : IDisposable
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly TextWriter _originalOut;
    private readonly StringWriter _writer;

    public DisableCommandTests()
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
    public async Task Disable_WithSetting_DisablesSetting()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--setting", "TEST-001" });

        var output = GetOutput();
        Assert.Contains("Setting 'TEST-001' disabled successfully.", output);
        _mockRegistry.Verify(r => r.SetValue(
            "HKLM", @"SOFTWARE\Test", "TestValue1", 0, SettingValueType.DWord), Times.Once);
    }

    [Fact]
    public async Task Disable_WithInvalidSetting_ShowsNotFound()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--setting", "INVALID-999" });

        var output = GetOutput();
        Assert.Contains("Setting 'INVALID-999' not found.", output);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Never);
    }

    [Fact]
    public async Task Disable_WithCategory_DisablesCategorySettings()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--category", "WindowsDefender" });

        var output = GetOutput();
        Assert.Contains("Disabled 2 settings in category 'WindowsDefender'.", output);
    }

    [Fact]
    public async Task Disable_WithAll_DisablesAllSettings()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--all" });

        var output = GetOutput();
        Assert.Contains("Disabled 3 security settings.", output);
        _mockRegistry.Verify(r => r.SetValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Disable_NoOption_ShowsUsageMessage()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = GetOutput();
        Assert.Contains("Please specify --setting, --category, or --all.", output);
    }

    [Fact]
    public async Task Disable_WithAllDryRun_ShowsPreview()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--all", "--dry-run" });

        var output = GetOutput();
        Assert.Contains("DRY RUN", output);
        Assert.Contains("TEST-001", output);
        Assert.Contains("TEST-002", output);
        Assert.Contains("TEST-003", output);
        Assert.Contains("Total: 3 settings would be changed.", output);
    }

    [Fact]
    public async Task Disable_WithSettingDryRun_ShowsPreview()
    {
        var cmd = DisableCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--setting", "TEST-001", "--dry-run" });

        var output = GetOutput();
        Assert.Contains("DRY RUN", output);
        Assert.Contains("TEST-001", output);
        Assert.Contains("Total: 1 settings would be changed.", output);
    }
}
