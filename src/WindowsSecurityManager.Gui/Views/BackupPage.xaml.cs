using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsSecurityManager.Gui.ViewModels;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Gui.Views;

/// <summary>
/// Backup and restore page for saving/loading security setting state.
/// </summary>
public sealed partial class BackupPage : Page
{
    private readonly BackupViewModel _viewModel = new();

    public BackupPage()
    {
        InitializeComponent();

        foreach (var cat in Enum.GetValues<SecurityCategory>())
            BackupCategoryFilter.Items.Add(cat);
    }

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedCategory = BackupCategoryFilter.SelectedItem is SecurityCategory cat ? cat : null;
        await _viewModel.CreateBackupAsync();
        ShowStatus(_viewModel.StatusMessage);
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.RestoreFromFileAsync(RestorePathBox.Text);
        ShowStatus(_viewModel.StatusMessage);
    }

    private void ShowStatus(string message)
    {
        StatusBar.Message = message;
        StatusBar.IsOpen = true;
    }
}
