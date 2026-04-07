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
    public partial double OverallCompliance { get; set; }

    [ObservableProperty]
    public partial int TotalSettings { get; set; }

    [ObservableProperty]
    public partial int EnabledCount { get; set; }

    [ObservableProperty]
    public partial int DisabledCount { get; set; }

    [ObservableProperty]
    public partial int NotConfiguredCount { get; set; }

    [ObservableProperty]
    public partial List<CategoryComplianceItem> Categories { get; set; } = [];

    [RelayCommand]
    public void Refresh()
    {
        // Generate a single report for all settings to avoid duplicate registry IO
        var report = _manager.GenerateReport();
        OverallCompliance = report.CompliancePercentage;
        TotalSettings = report.TotalSettings;
        EnabledCount = report.EnabledCount;
        DisabledCount = report.DisabledCount - report.NotConfiguredCount;
        NotConfiguredCount = report.NotConfiguredCount;

        // Compute per-category aggregates from the same in-memory statuses
        var categoryItems = new List<CategoryComplianceItem>();
        var grouped = report.Settings.GroupBy(s => s.Setting.Category);
        foreach (var group in grouped)
        {
            var catStatuses = group.ToList();
            var catEnabled = catStatuses.Count(s => s.IsEnabled);
            var catTotal = catStatuses.Count;
            categoryItems.Add(new CategoryComplianceItem
            {
                Category = group.Key,
                DisplayName = FormatCategoryName(group.Key),
                CompliancePercentage = catTotal > 0 ? Math.Round((double)catEnabled / catTotal * 100, 1) : 0,
                EnabledCount = catEnabled,
                TotalCount = catTotal
            });
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
