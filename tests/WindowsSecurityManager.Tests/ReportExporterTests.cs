using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

public class ReportExporterTests
{
    private static SecurityReport CreateTestReport()
    {
        var settings = new List<SettingStatus>
        {
            new SettingStatus
            {
                Setting = new SecuritySetting
                {
                    Id = "TEST-001",
                    Name = "Test Setting",
                    Description = "A test setting",
                    Category = SecurityCategory.WindowsDefender,
                    RegistryHive = "HKLM",
                    RegistryPath = @"SOFTWARE\Test",
                    ValueName = "TestValue",
                    ValueType = SettingValueType.DWord,
                    EnabledValue = 1,
                    DisabledValue = 0,
                    RecommendedValue = 1
                },
                CurrentValue = 1,
                IsEnabled = true,
                IsConfigured = true,
                MatchesRecommended = true
            },
            new SettingStatus
            {
                Setting = new SecuritySetting
                {
                    Id = "TEST-002",
                    Name = "Disabled Setting",
                    Description = "A disabled setting",
                    Category = SecurityCategory.Firewall,
                    RegistryHive = "HKLM",
                    RegistryPath = @"SOFTWARE\Test2",
                    ValueName = "TestValue2",
                    ValueType = SettingValueType.DWord,
                    EnabledValue = 1,
                    DisabledValue = 0,
                    RecommendedValue = 1
                },
                CurrentValue = null,
                IsEnabled = false,
                IsConfigured = false,
                MatchesRecommended = false
            }
        };

        return new SecurityReport { Settings = settings };
    }

    [Fact]
    public void ExportJson_ContainsRequiredFields()
    {
        var exporter = new ReportExporter();
        var report = CreateTestReport();

        var json = exporter.Export(report, ExportFormat.Json);

        Assert.Contains("\"TotalSettings\": 2", json);
        Assert.Contains("\"EnabledCount\": 1", json);
        Assert.Contains("\"CompliancePercentage\": 50", json);
        Assert.Contains("TEST-001", json);
        Assert.Contains("TEST-002", json);
    }

    [Fact]
    public void ExportCsv_ContainsHeaders()
    {
        var exporter = new ReportExporter();
        var report = CreateTestReport();

        var csv = exporter.Export(report, ExportFormat.Csv);

        Assert.StartsWith("Id,Name,Category,Status", csv);
    }

    [Fact]
    public void ExportCsv_ContainsData()
    {
        var exporter = new ReportExporter();
        var report = CreateTestReport();

        var csv = exporter.Export(report, ExportFormat.Csv);

        Assert.Contains("TEST-001", csv);
        Assert.Contains("Enabled", csv);
        Assert.Contains("Missing", csv);
    }

    [Fact]
    public void ExportHtml_ContainsHtmlStructure()
    {
        var exporter = new ReportExporter();
        var report = CreateTestReport();

        var html = exporter.Export(report, ExportFormat.Html);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<table>", html);
        Assert.Contains("TEST-001", html);
        Assert.Contains("Compliance Report", html);
    }

    [Fact]
    public void ExportToFile_WritesFile()
    {
        var exporter = new ReportExporter();
        var report = CreateTestReport();
        var path = Path.Combine(Path.GetTempPath(), $"test-export-{Guid.NewGuid()}.json");

        try
        {
            exporter.ExportToFile(report, ExportFormat.Json, path);

            Assert.True(File.Exists(path));
            var content = File.ReadAllText(path);
            Assert.Contains("TEST-001", content);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ExportCsv_EscapesCommasInValues()
    {
        var exporter = new ReportExporter();
        var settings = new List<SettingStatus>
        {
            new SettingStatus
            {
                Setting = new SecuritySetting
                {
                    Id = "TEST-001",
                    Name = "Setting with, comma",
                    Description = "Comma test",
                    Category = SecurityCategory.WindowsDefender,
                    RegistryHive = "HKLM",
                    RegistryPath = @"SOFTWARE\Test",
                    ValueName = "Test",
                    ValueType = SettingValueType.DWord,
                    EnabledValue = 1,
                    DisabledValue = 0,
                    RecommendedValue = 1
                },
                CurrentValue = 1,
                IsEnabled = true,
                IsConfigured = true,
                MatchesRecommended = true
            }
        };

        var report = new SecurityReport { Settings = settings };
        var csv = exporter.Export(report, ExportFormat.Csv);

        Assert.Contains("\"Setting with, comma\"", csv);
    }
}
