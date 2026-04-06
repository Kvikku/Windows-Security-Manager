using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Gui.ViewModels;

/// <summary>
/// ViewModel for the Backup/Restore page.
/// </summary>
public partial class BackupViewModel : ObservableObject
{
    private readonly SecuritySettingsManager _manager;
    private readonly IRegistryService _registryService;

    public BackupViewModel()
    {
        _manager = App.SettingsManager;
        _registryService = App.RegistryService;
        CategoryOptions = [.. Enum.GetValues<SecurityCategory>()];
    }

    [ObservableProperty]
    private SecurityCategory? _selectedCategory;

    [ObservableProperty]
    private List<SecurityCategory> _categoryOptions = [];

    [ObservableProperty]
    private string _statusMessage = "";

    [RelayCommand]
    public async Task CreateBackupAsync()
    {
        var service = new BackupService(_manager, _registryService);
        var backup = service.CreateBackup(SelectedCategory);

        var fileName = $"wsm-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json";
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, fileName);

        await Task.Run(() => service.SaveToFile(backup, filePath));
        StatusMessage = $"Backup saved: {filePath} ({backup.Entries.Count} settings)";
    }

    [RelayCommand]
    public async Task RestoreFromFileAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "No file selected";
            return;
        }

        try
        {
            var service = new BackupService(_manager, _registryService);
            var count = await Task.Run(() => service.RestoreFromFile(filePath));
            StatusMessage = $"Restored {count} settings from backup";
        }
        catch (FileNotFoundException)
        {
            StatusMessage = "Backup file not found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Restore failed: {ex.Message}";
        }
    }
}
