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
    private CancellationTokenSource? _searchCts;

    public SettingsViewModel()
    {
        _manager = App.SettingsManager;
        CategoryOptions = [.. Enum.GetValues<SecurityCategory>()];
        ProfileOptions = SecurityProfiles.GetProfiles().ToList();
        _ = RefreshSettingsAsync();
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

    /// <summary>
    /// Indicates whether the last enable/disable operation succeeded.
    /// </summary>
    [ObservableProperty]
    private bool _lastOperationSucceeded;

    partial void OnSelectedCategoryChanged(SecurityCategory? value) => _ = RefreshSettingsAsync();
    partial void OnSearchTextChanged(string value) => _ = DebouncedRefreshAsync();

    /// <summary>
    /// Debounces search input to avoid triggering registry reads on every keystroke.
    /// </summary>
    private async Task DebouncedRefreshAsync()
    {
        var oldCts = _searchCts;
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        // Dispose the previous CancellationTokenSource after cancelling
        oldCts?.Cancel();
        oldCts?.Dispose();

        try
        {
            await Task.Delay(300, token);
            if (!token.IsCancellationRequested)
                await RefreshSettingsAsync();
        }
        catch (TaskCanceledException)
        {
            // Expected when a new keystroke cancels the previous delay
        }
    }

    [RelayCommand]
    public async Task RefreshSettingsAsync()
    {
        var searchText = SearchText;
        var category = SelectedCategory;

        var items = await Task.Run(() =>
        {
            var allSettings = string.IsNullOrWhiteSpace(searchText)
                ? _manager.GetSettings(category)
                : _manager.SearchSettings(searchText);

            if (category.HasValue && !string.IsNullOrWhiteSpace(searchText))
            {
                allSettings = allSettings
                    .Where(s => s.Category == category.Value)
                    .ToList();
            }

            return allSettings.Select(s =>
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
        });

        Settings = items;
    }

    [RelayCommand]
    public void EnableSetting(string settingId)
    {
        LastOperationSucceeded = _manager.EnableSetting(settingId);
        StatusMessage = LastOperationSucceeded
            ? $"Enabled: {settingId}"
            : $"Failed to enable: {settingId}";
        if (LastOperationSucceeded)
            _ = RefreshSettingsAsync();
    }

    [RelayCommand]
    public void DisableSetting(string settingId)
    {
        LastOperationSucceeded = _manager.DisableSetting(settingId);
        StatusMessage = LastOperationSucceeded
            ? $"Disabled: {settingId}"
            : $"Failed to disable: {settingId}";
        if (LastOperationSucceeded)
            _ = RefreshSettingsAsync();
    }

    [RelayCommand]
    public void EnableAll()
    {
        var count = SelectedCategory.HasValue
            ? _manager.EnableCategory(SelectedCategory.Value)
            : _manager.EnableAll();
        StatusMessage = $"Enabled {count} settings";
        _ = RefreshSettingsAsync();
    }

    [RelayCommand]
    public void DisableAll()
    {
        var count = SelectedCategory.HasValue
            ? _manager.DisableCategory(SelectedCategory.Value)
            : _manager.DisableAll();
        StatusMessage = $"Disabled {count} settings";
        _ = RefreshSettingsAsync();
    }

    [RelayCommand]
    public void ApplyProfile(SecurityProfile? profile)
    {
        if (profile == null) return;
        var count = _manager.EnableSettings(profile.SettingIds);
        StatusMessage = $"Applied profile '{profile.Name}': {count} settings enabled";
        _ = RefreshSettingsAsync();
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
