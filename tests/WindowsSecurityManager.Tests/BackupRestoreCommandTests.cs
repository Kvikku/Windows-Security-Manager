using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class BackupRestoreCommandTests : IDisposable
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly AuditLogger _auditLogger;
    private readonly TextWriter _originalOut;
    private readonly StringWriter _writer;
    private readonly string _auditLogPath;

    public BackupRestoreCommandTests()
    {
        _mockRegistry = new Mock<IRegistryService>();
        _manager = new SecuritySettingsManager(_mockRegistry.Object, new[] { new TestSettingProvider() });
        _auditLogPath = Path.Combine(Path.GetTempPath(), $"wsm-test-audit-{Guid.NewGuid()}.log");
        _auditLogger = new AuditLogger(_auditLogPath);
        _originalOut = Console.Out;
        _writer = new StringWriter();
        Console.SetOut(_writer);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOut);
        _writer.Dispose();
        if (File.Exists(_auditLogPath)) File.Delete(_auditLogPath);
    }

    private string GetOutput() => _writer.ToString();

    [Fact]
    public async Task Backup_WithOutput_CreatesBackupFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var cmd = BackupCommand.Create(_manager, _mockRegistry.Object, _auditLogger);

            await cmd.InvokeAsync(new[] { "--output", filePath });

            var output = GetOutput();
            Assert.Contains($"Backup saved to '{filePath}'", output);
            Assert.Contains("3 settings", output);
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task Backup_WithCategory_FiltersSettings()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var cmd = BackupCommand.Create(_manager, _mockRegistry.Object, _auditLogger);

            await cmd.InvokeAsync(new[] { "--output", filePath, "--category", "WindowsDefender" });

            var output = GetOutput();
            Assert.Contains($"Backup saved to '{filePath}'", output);
            Assert.Contains("2 settings", output);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task Backup_WithNullAuditLogger_DoesNotThrow()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var cmd = BackupCommand.Create(_manager, _mockRegistry.Object, null);

            await cmd.InvokeAsync(new[] { "--output", filePath });

            var output = GetOutput();
            Assert.Contains("Backup saved to", output);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task Restore_ValidFile_RestoresSettings()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            // First create a backup to restore from
            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var backupService = new BackupService(_manager, _mockRegistry.Object);
            var backup = backupService.CreateBackup();
            backupService.SaveToFile(backup, filePath);

            _mockRegistry.Reset();

            var cmd = RestoreCommand.Create(_manager, _mockRegistry.Object, _auditLogger);

            await cmd.InvokeAsync(new[] { filePath });

            var output = GetOutput();
            Assert.Contains("Restored 3 settings", output);
            Assert.Contains(filePath, output);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task Restore_InvalidFile_ShowsError()
    {
        var cmd = RestoreCommand.Create(_manager, _mockRegistry.Object, _auditLogger);

        await cmd.InvokeAsync(new[] { "/nonexistent/backup.json" });

        var output = GetOutput();
        Assert.Contains("Backup file not found", output);
    }
}
