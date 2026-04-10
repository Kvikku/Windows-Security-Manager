using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Gui.ViewModels;

/// <summary>
/// ViewModel for the Report page with export functionality.
/// </summary>
public partial class ReportViewModel : ObservableObject
{
    private readonly SecuritySettingsManager _manager;

    public ReportViewModel()
    {
        _manager = App.SettingsManager;
        CategoryOptions = [.. Enum.GetValues<SecurityCategory>()];
    }

    [ObservableProperty]
    private SecurityCategory? _selectedCategory;

    [ObservableProperty]
    private List<SecurityCategory> _categoryOptions = [];

    [ObservableProperty]
    private SecurityReport? _report;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private ExportFormat _selectedFormat = ExportFormat.Html;

    [RelayCommand]
    public async Task GenerateReportAsync()
    {
        StatusMessage = "Generating report...";
        var category = SelectedCategory;
        var report = await Task.Run(() => _manager.GenerateReport(category));
        Report = report;
        StatusMessage = $"Report generated: {report.TotalSettings} settings, {report.CompliancePercentage:F1}% compliance";
    }

    [RelayCommand]
    public async Task ExportReportAsync()
    {
        if (Report == null)
        {
            StatusMessage = "Generate a report first";
            return;
        }

        try
        {
            var exporter = new ReportExporter();
            var extension = SelectedFormat switch
            {
                ExportFormat.Json => "json",
                ExportFormat.Csv => "csv",
                ExportFormat.Html => "html",
                _ => "txt"
            };

            var fileName = $"wsm-report-{DateTime.Now:yyyyMMdd-HHmmss}.{extension}";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, fileName);
            var report = Report;
            var format = SelectedFormat;

            await Task.Run(() => exporter.ExportToFile(report, format, filePath));
            StatusMessage = $"Report exported to: {filePath}";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or DirectoryNotFoundException)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }
}
