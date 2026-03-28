using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

public class AuditLoggerTests
{
    private string GetTempLogPath() => Path.Combine(Path.GetTempPath(), $"wsm-test-{Guid.NewGuid()}.log");

    [Fact]
    public void Log_CreatesFile()
    {
        var path = GetTempLogPath();
        try
        {
            var logger = new AuditLogger(path);
            logger.Log("Enable", "TEST-001", "Enabled test setting");

            Assert.True(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Log_WritesCorrectFormat()
    {
        var path = GetTempLogPath();
        try
        {
            var logger = new AuditLogger(path);
            logger.Log("Enable", "TEST-001", "Details here");

            var content = File.ReadAllText(path);
            Assert.Contains("Enable: TEST-001", content);
            Assert.Contains("Details here", content);
            Assert.Matches(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\]", content);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Log_AppendsMultipleEntries()
    {
        var path = GetTempLogPath();
        try
        {
            var logger = new AuditLogger(path);
            logger.Log("Enable", "TEST-001");
            logger.Log("Disable", "TEST-002");
            logger.Log("EnableAll", "All settings");

            var lines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            Assert.Equal(3, lines.Length);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ReadAll_ReturnsEntries()
    {
        var path = GetTempLogPath();
        try
        {
            var logger = new AuditLogger(path);
            logger.Log("Enable", "TEST-001", "Detail 1");
            logger.Log("Disable", "TEST-002");

            var entries = logger.ReadAll();
            Assert.Equal(2, entries.Count);
            Assert.Equal("Enable", entries[0].Action);
            Assert.Equal("TEST-001", entries[0].Target);
            Assert.Equal("Detail 1", entries[0].Details);
            Assert.Equal("Disable", entries[1].Action);
            Assert.Null(entries[1].Details);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ReadAll_EmptyFile_ReturnsEmpty()
    {
        var path = GetTempLogPath();
        try
        {
            var logger = new AuditLogger(path);
            var entries = logger.ReadAll();
            Assert.Empty(entries);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Log_WithoutDetails_OmitsPipe()
    {
        var path = GetTempLogPath();
        try
        {
            var logger = new AuditLogger(path);
            logger.Log("EnableAll", "All settings");

            var content = File.ReadAllText(path);
            Assert.Contains("EnableAll: All settings", content);
            Assert.DoesNotContain("|", content);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
