using Moq;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

public class BackupServiceTests
{
    private readonly Mock<IRegistryService> _mockRegistry;
    private readonly SecuritySettingsManager _manager;
    private readonly BackupService _backupService;

    public BackupServiceTests()
    {
        _mockRegistry = new Mock<IRegistryService>();
        var provider = new TestSettingProvider();
        _manager = new SecuritySettingsManager(_mockRegistry.Object, new[] { provider });
        _backupService = new BackupService(_manager, _mockRegistry.Object);
    }

    [Fact]
    public void CreateBackup_CapturesAllSettings()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns(0);
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "FwValue1")).Returns((object?)null);

        var backup = _backupService.CreateBackup();

        Assert.Equal(3, backup.Entries.Count);
    }

    [Fact]
    public void CreateBackup_RecordsConfiguredValues()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);

        var backup = _backupService.CreateBackup();
        var entry = backup.Entries.First(e => e.SettingId == "TEST-001");

        Assert.True(entry.WasConfigured);
        Assert.Equal(1, entry.Value);
    }

    [Fact]
    public void CreateBackup_RecordsUnconfiguredValues()
    {
        _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns((object?)null);

        var backup = _backupService.CreateBackup();
        var entry = backup.Entries.First(e => e.SettingId == "TEST-001");

        Assert.False(entry.WasConfigured);
        Assert.Null(entry.Value);
    }

    [Fact]
    public void CreateBackup_FiltersByCategory()
    {
        var backup = _backupService.CreateBackup(SecurityCategory.WindowsDefender);
        Assert.Equal(2, backup.Entries.Count);
    }

    [Fact]
    public void SaveToFile_CreatesFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var backup = _backupService.CreateBackup();
            _backupService.SaveToFile(backup, path);

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
    public void RestoreFromFile_RestoresValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            _mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1);

            var backup = _backupService.CreateBackup();
            _backupService.SaveToFile(backup, path);

            // Reset mock to verify restore calls
            _mockRegistry.Reset();

            int count = _backupService.RestoreFromFile(path);

            Assert.Equal(3, count);
            _mockRegistry.Verify(r => r.SetValue(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Exactly(3));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void RestoreFromFile_DeletesUnconfiguredValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"wsm-test-backup-{Guid.NewGuid()}.json");
        try
        {
            // Only TEST-001 is configured, others are null
            _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue1")).Returns(1);
            _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "TestValue2")).Returns((object?)null);
            _mockRegistry.Setup(r => r.GetValue("HKLM", @"SOFTWARE\Test", "FwValue1")).Returns((object?)null);

            var backup = _backupService.CreateBackup();
            _backupService.SaveToFile(backup, path);

            _mockRegistry.Reset();

            int count = _backupService.RestoreFromFile(path);

            Assert.Equal(3, count);
            // 1 SetValue for the configured one, 2 DeleteValue for unconfigured
            _mockRegistry.Verify(r => r.SetValue(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<SettingValueType>()), Times.Once);
            _mockRegistry.Verify(r => r.DeleteValue(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void RestoreFromFile_ThrowsOnMissingFile()
    {
        Assert.Throws<FileNotFoundException>(() =>
            _backupService.RestoreFromFile("/nonexistent/backup.json"));
    }
}
