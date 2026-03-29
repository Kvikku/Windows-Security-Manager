using System.CommandLine;
using Moq;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

[Collection("Console")]
public class BackupRestoreCommandTests : IDisposable
{
    private readonly CommandTestContext _context;
    private readonly AuditLogger _auditLogger;
    private readonly string _auditLogPath;

    public BackupRestoreCommandTests()
    {
        _context = new CommandTestContext();
        _auditLogPath = Path.Combine(Path.GetTempPath(), $"wsm-test-audit-{Guid.NewGuid()}.log");
        _auditLogger = new AuditLogger(_auditLogPath);
    }

    public void Dispose()
    {
        _context.Dispose();
        if (File.Exists(_auditLogPath)) File.Delete(_auditLogPath);
    }

    [Fact]
    public async Task Backup_WithOutput_CreatesBackupFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            _context.MockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var cmd = BackupCommand.Create(_context.Manager, _context.MockRegistry.Object, _auditLogger);

            await cmd.InvokeAsync(new[] { "--output", filePath });

            var output = _context.GetOutput();
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
            _context.MockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var cmd = BackupCommand.Create(_context.Manager, _context.MockRegistry.Object, _auditLogger);

            await cmd.InvokeAsync(new[] { "--output", filePath, "--category", "WindowsDefender" });

            var output = _context.GetOutput();
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
            _context.MockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var cmd = BackupCommand.Create(_context.Manager, _context.MockRegistry.Object, null);

            await cmd.InvokeAsync(new[] { "--output", filePath });

            var output = _context.GetOutput();
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
            _context.MockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var backupService = new BackupService(_context.Manager, _context.MockRegistry.Object);
            var backup = backupService.CreateBackup();
            backupService.SaveToFile(backup, filePath);

            _context.MockRegistry.Reset();

            var cmd = RestoreCommand.Create(_context.Manager, _context.MockRegistry.Object, _auditLogger);

            await cmd.InvokeAsync(new[] { filePath });

            var output = _context.GetOutput();
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
        var filePath = Path.Combine(Path.GetTempPath(), $"wsm-nonexistent-backup-{Guid.NewGuid()}.json");
        var cmd = RestoreCommand.Create(_context.Manager, _context.MockRegistry.Object, _auditLogger);

        await cmd.InvokeAsync(new[] { filePath });

        var output = _context.GetOutput();
        Assert.Contains("Backup file not found", output);
    }
}
