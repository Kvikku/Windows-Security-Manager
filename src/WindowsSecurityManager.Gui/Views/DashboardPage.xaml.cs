using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsSecurityManager.Gui.ViewModels;

namespace WindowsSecurityManager.Gui.Views;

/// <summary>
/// Dashboard page showing overall compliance and category breakdown.
/// </summary>
public sealed partial class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel = new();

    public DashboardPage()
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
        ComplianceText.Text = $"{_viewModel.OverallCompliance:F1}%";
        ComplianceBar.Value = _viewModel.OverallCompliance;
        EnabledText.Text = $"{_viewModel.EnabledCount} of {_viewModel.TotalSettings}";
        DisabledText.Text = _viewModel.DisabledCount.ToString();
        NotConfiguredText.Text = _viewModel.NotConfiguredCount.ToString();
        CategoryRepeater.ItemsSource = _viewModel.Categories;
    }
}
