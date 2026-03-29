using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class ReportCommandTests : IDisposable
{
    private readonly CommandTestContext _context;

    public ReportCommandTests()
    {
        _context = new CommandTestContext();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Report_Default_ShowsConsoleSummary()
    {
        _context.MockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _context.MockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(0);
        _context.MockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "FwValue1")).Returns((object?)null);

        var cmd = ReportCommand.Create(_context.Manager);

        await cmd.InvokeAsync(Array.Empty<string>());

        var output = _context.GetOutput();
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
        _context.MockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _context.MockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(1);

        var cmd = ReportCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--category", "WindowsDefender" });

        var output = _context.GetOutput();
        Assert.Contains("Compliance Report", output);
        Assert.Contains("WindowsDefender", output);
        Assert.DoesNotContain("TEST-003", output);
    }

    [Fact]
    public async Task Report_WithFormatJson_OutputsJson()
    {
        var cmd = ReportCommand.Create(_context.Manager);

        await cmd.InvokeAsync(new[] { "--format", "Json" });

        var output = _context.GetOutput();
        Assert.Contains("GeneratedAt", output);
        Assert.Contains("Settings", output);
    }

    [Fact]
    public async Task Report_WithFormatAndOutput_ExportsToFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-report-{Guid.NewGuid()}.json");
        try
        {
            var cmd = ReportCommand.Create(_context.Manager);

            await cmd.InvokeAsync(new[] { "--format", "Json", "--output", filePath });

            var output = _context.GetOutput();
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
