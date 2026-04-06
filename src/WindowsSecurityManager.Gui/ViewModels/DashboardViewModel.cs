using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Gui.ViewModels;

/// <summary>
/// ViewModel for the Dashboard page showing compliance overview per category.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly SecuritySettingsManager _manager;

    public DashboardViewModel()
    {
        _manager = App.SettingsManager;
        Refresh();
    }

    [ObservableProperty]
    private double _overallCompliance;

    [ObservableProperty]
    private int _totalSettings;

    [ObservableProperty]
    private int _enabledCount;

    [ObservableProperty]
    private int _disabledCount;

    [ObservableProperty]
    private int _notConfiguredCount;

    [ObservableProperty]
    private List<CategoryComplianceItem> _categories = [];

    [RelayCommand]
    public void Refresh()
    {
        var report = _manager.GenerateReport();
        OverallCompliance = report.CompliancePercentage;
        TotalSettings = report.TotalSettings;
        EnabledCount = report.EnabledCount;
        DisabledCount = report.DisabledCount - report.NotConfiguredCount;
        NotConfiguredCount = report.NotConfiguredCount;

        var categoryItems = new List<CategoryComplianceItem>();
        foreach (var category in Enum.GetValues<SecurityCategory>())
        {
            var catReport = _manager.GenerateReport(category);
            if (catReport.TotalSettings > 0)
            {
                categoryItems.Add(new CategoryComplianceItem
                {
                    Category = category,
                    DisplayName = FormatCategoryName(category),
                    CompliancePercentage = catReport.CompliancePercentage,
                    EnabledCount = catReport.EnabledCount,
                    TotalCount = catReport.TotalSettings
                });
            }
        }
        Categories = categoryItems;
    }

    private static string FormatCategoryName(SecurityCategory category)
    {
        return category switch
        {
            SecurityCategory.WindowsDefender => "Windows Defender",
            SecurityCategory.AttackSurfaceReduction => "Attack Surface Reduction",
            SecurityCategory.Firewall => "Firewall",
            SecurityCategory.CisBenchmark => "CIS Benchmark",
            SecurityCategory.AccountPolicy => "Account Policy",
            SecurityCategory.NetworkSecurity => "Network Security",
            _ => category.ToString()
        };
    }
}

/// <summary>
/// Represents compliance data for a single category on the dashboard.
/// </summary>
public class CategoryComplianceItem
{
    public SecurityCategory Category { get; set; }
    public string DisplayName { get; set; } = "";
    public double CompliancePercentage { get; set; }
    public int EnabledCount { get; set; }
    public int TotalCount { get; set; }
}
