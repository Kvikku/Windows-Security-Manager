using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsSecurityManager.Gui.ViewModels;

namespace WindowsSecurityManager.Gui.Views;

/// <summary>
/// Audit log viewer page showing timestamped action history.
/// </summary>
public sealed partial class AuditLogPage : Page
{
    private readonly AuditLogViewModel _viewModel = new();

    public AuditLogPage()
    {
        InitializeComponent();
        UpdateUI();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Refresh();
        UpdateUI();
    }

    private void UpdateUI()
    {
        LogList.ItemsSource = _viewModel.Entries.Select(e => new
        {
            TimestampDisplay = e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            e.Action,
            e.Target,
            Details = e.Details ?? ""
        }).ToList();

        CountText.Text = _viewModel.StatusMessage;
    }
}
