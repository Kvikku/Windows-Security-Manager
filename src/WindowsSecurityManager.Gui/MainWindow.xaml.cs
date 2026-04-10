using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsSecurityManager.Gui.Views;

namespace WindowsSecurityManager.Gui;

/// <summary>
/// Main application window with NavigationView shell.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Title = "Windows Security Manager";
        ExtendsContentIntoTitleBar = true;

        // Navigate to Dashboard by default
        ContentFrame.Navigate(typeof(DashboardPage));
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item)
            return;

        var tag = item.Tag?.ToString();
        var pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Settings" => typeof(SettingsPage),
            "Reports" => typeof(ReportPage),
            "Backup" => typeof(BackupPage),
            "AuditLog" => typeof(AuditLogPage),
            _ => null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
