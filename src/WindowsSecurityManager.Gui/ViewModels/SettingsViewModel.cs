using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Gui.ViewModels;

/// <summary>
/// ViewModel for the Settings page with search, category filter, and enable/disable.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SecuritySettingsManager _manager;

    public SettingsViewModel()
    {
        _manager = App.SettingsManager;
        CategoryOptions = [.. Enum.GetValues<SecurityCategory>()];
        ProfileOptions = SecurityProfiles.GetProfiles().ToList();
        RefreshSettings();
    }

    [ObservableProperty]
    private List<SettingItemViewModel> _settings = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearCategoryFilterCommand))]
    private SecurityCategory? _selectedCategory;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private List<SecurityCategory> _categoryOptions = [];

    [ObservableProperty]
    private List<SecurityProfile> _profileOptions = [];

    [ObservableProperty]
    private string _statusMessage = "";

    partial void OnSelectedCategoryChanged(SecurityCategory? value) => RefreshSettings();
    partial void OnSearchTextChanged(string value) => RefreshSettings();

    [RelayCommand]
    public void RefreshSettings()
    {
        var allSettings = string.IsNullOrWhiteSpace(SearchText)
            ? _manager.GetSettings(SelectedCategory)
            : _manager.SearchSettings(SearchText);

        if (SelectedCategory.HasValue && !string.IsNullOrWhiteSpace(SearchText))
        {
            allSettings = allSettings
                .Where(s => s.Category == SelectedCategory.Value)
                .ToList();
        }

        Settings = allSettings.Select(s =>
        {
            var status = _manager.GetSettingStatus(s);
            return new SettingItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Category = s.Category,
                IsEnabled = status.IsEnabled,
                IsConfigured = status.IsConfigured,
                CurrentValue = status.CurrentValue?.ToString() ?? "Not configured",
                ExpectedValue = s.EnabledValue.ToString() ?? "",
                MatchesRecommended = status.MatchesRecommended
            };
        }).ToList();
    }

    [RelayCommand]
    public void EnableSetting(string settingId)
    {
        if (_manager.EnableSetting(settingId))
        {
            StatusMessage = $"Enabled: {settingId}";
            RefreshSettings();
        }
    }

    [RelayCommand]
    public void DisableSetting(string settingId)
    {
        if (_manager.DisableSetting(settingId))
        {
            StatusMessage = $"Disabled: {settingId}";
            RefreshSettings();
        }
    }

    [RelayCommand]
    public void EnableAll()
    {
        var count = SelectedCategory.HasValue
            ? _manager.EnableCategory(SelectedCategory.Value)
            : _manager.EnableAll();
        StatusMessage = $"Enabled {count} settings";
        RefreshSettings();
    }

    [RelayCommand]
    public void DisableAll()
    {
        var count = SelectedCategory.HasValue
            ? _manager.DisableCategory(SelectedCategory.Value)
            : _manager.DisableAll();
        StatusMessage = $"Disabled {count} settings";
        RefreshSettings();
    }

    [RelayCommand]
    public void ApplyProfile(SecurityProfile? profile)
    {
        if (profile == null) return;
        var count = _manager.EnableSettings(profile.SettingIds);
        StatusMessage = $"Applied profile '{profile.Name}': {count} settings enabled";
        RefreshSettings();
    }

    [RelayCommand(CanExecute = nameof(CanClearCategoryFilter))]
    public void ClearCategoryFilter()
    {
        SelectedCategory = null;
    }

    private bool CanClearCategoryFilter() => SelectedCategory.HasValue;
}

/// <summary>
/// ViewModel representing a single security setting row in the list.
/// </summary>
public partial class SettingItemViewModel : ObservableObject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public SecurityCategory Category { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsConfigured { get; set; }
    public string CurrentValue { get; set; } = "";
    public string ExpectedValue { get; set; } = "";
    public bool MatchesRecommended { get; set; }

    public string StatusText => IsEnabled ? "Enabled" : IsConfigured ? "Disabled" : "Missing";
}
