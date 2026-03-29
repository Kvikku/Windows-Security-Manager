using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class ReportCommandTests : IDisposable
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly TextWriter _originalOut;
    private readonly StringWriter _writer;

    public ReportCommandTests()
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
    public async Task Report_Default_ShowsConsoleSummary()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(0);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "FwValue1")).Returns((object?)null);

        var cmd = ReportCommand.Create(_manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = GetOutput();
        Assert.Contains("Compliance Report", output);
        Assert.Contains("Total Settings:", output);
        Assert.Contains("Enabled (Hardened):", output);
        Assert.Contains("Disabled:", output);
        Assert.Contains("Not Configured:", output);
        Assert.Contains("Compliance:", output);
    }

    [Fact]
    public async Task Report_WithCategory_FiltersReport()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(1);

        var cmd = ReportCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--category", "WindowsDefender" });

        var output = GetOutput();
        Assert.Contains("Compliance Report", output);
        Assert.Contains("WindowsDefender", output);
        Assert.DoesNotContain("TEST-003", output);
    }

    [Fact]
    public async Task Report_WithFormatJson_OutputsJson()
    {
        var cmd = ReportCommand.Create(_manager);

        await cmd.InvokeAsync(new[] { "--format", "Json" });

        var output = GetOutput();
        Assert.Contains("GeneratedAt", output);
        Assert.Contains("Settings", output);
    }

    [Fact]
    public async Task Report_WithFormatAndOutput_ExportsToFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-report-{Guid.NewGuid()}.json");
        try
        {
            var cmd = ReportCommand.Create(_manager);

            await cmd.InvokeAsync(new[] { "--format", "Json", "--output", filePath });

            var output = GetOutput();
            Assert.Contains($"Report exported to '{filePath}'", output);
            Assert.Contains("Json format", output);
            Assert.True(File.Exists(filePath));

            var content = File.ReadAllText(filePath);
            Assert.Contains("GeneratedAt", content);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
