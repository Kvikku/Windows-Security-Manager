using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Gui.ViewModels;

/// <summary>
/// ViewModel for the Audit Log page.
/// </summary>
public partial class AuditLogViewModel : ObservableObject
{
    public AuditLogViewModel()
    {
        Refresh();
    }

    [ObservableProperty]
    public partial List<AuditLogEntry> Entries { get; set; } = [];

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";

    [RelayCommand]
    public void Refresh()
    {
        var logger = App.AuditLogger;
        if (logger == null)
        {
            StatusMessage = "Audit logging is not available";
            Entries = [];
            return;
        }

        var allEntries = logger.ReadAll();
        // Show newest first
        Entries = allEntries.Reverse().ToList();
        StatusMessage = $"{Entries.Count} log entries";
    }
}
