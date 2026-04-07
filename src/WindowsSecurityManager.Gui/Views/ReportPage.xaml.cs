using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsSecurityManager.Gui.ViewModels;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Gui.Views;

/// <summary>
/// Report page for generating and exporting compliance reports.
/// </summary>
public sealed partial class ReportPage : Page
{
    private readonly ReportViewModel _viewModel = new();

    public ReportPage()
    {
        InitializeComponent();

        foreach (var cat in Enum.GetValues<SecurityCategory>())
            CategoryFilter.Items.Add(cat);
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.SelectedCategory = CategoryFilter.SelectedItem is SecurityCategory cat ? cat : null;
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.GenerateReportAsync();
        var report = _viewModel.Report;
        if (report == null) return;

        TotalText.Text = report.TotalSettings.ToString();
        HardenedText.Text = report.EnabledCount.ToString();
        MisconfiguredText.Text = (report.DisabledCount - report.NotConfiguredCount).ToString();
        ComplianceText.Text = $"{report.CompliancePercentage:F1}%";

        ReportList.ItemsSource = report.Settings.Select(s => new
        {
            StatusText = s.IsEnabled ? "✓ Enabled" : s.IsConfigured ? "✗ Disabled" : "— Missing",
            s.Setting.Id,
            s.Setting.Name,
            CurrentDisplay = $"Current: {s.CurrentValue ?? "N/A"}",
            ExpectedDisplay = $"Expected: {s.Setting.EnabledValue}"
        }).ToList();

        ReportContent.Visibility = Visibility.Visible;
        ShowStatus(_viewModel.StatusMessage);
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedFormat = FormatCombo.SelectedIndex switch
        {
            0 => ExportFormat.Json,
            1 => ExportFormat.Csv,
            _ => ExportFormat.Html
        };

        await _viewModel.ExportReportAsync();
        ShowStatus(_viewModel.StatusMessage);
    }

    private void ShowStatus(string message)
    {
        StatusBar.Message = message;
        StatusBar.IsOpen = true;
    }
}
