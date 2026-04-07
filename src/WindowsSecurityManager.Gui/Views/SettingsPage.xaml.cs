using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsSecurityManager.Gui.ViewModels;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Gui.Views;

/// <summary>
/// Settings page with search, filter, and enable/disable controls.
/// </summary>
public sealed partial class SettingsPage : Page
{
    private readonly SettingsViewModel _viewModel = new();

    public SettingsPage()
    {
        InitializeComponent();

        // Populate category filter (with "All" as null option)
        CategoryFilter.Items.Add("All Categories");
        foreach (var cat in _viewModel.CategoryOptions)
            CategoryFilter.Items.Add(cat);

        ProfileCombo.ItemsSource = _viewModel.ProfileOptions;

        UpdateList();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        _viewModel.SearchText = sender.Text;
        // Debounced refresh handled by ViewModel; update list when settings change
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.Settings))
        {
            UpdateList();
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryFilter.SelectedItem is SecurityCategory cat)
            _viewModel.SelectedCategory = cat;
        else
            _viewModel.SelectedCategory = null;
        UpdateList();
    }

    private void ClearFilter_Click(object sender, RoutedEventArgs e)
    {
        CategoryFilter.SelectedIndex = 0;
        _viewModel.SelectedCategory = null;
        UpdateList();
    }

    private void ProfileCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProfileCombo.SelectedItem is SecurityProfile profile)
        {
            _viewModel.ApplyProfile(profile);
            UpdateList();
            ShowStatus(_viewModel.StatusMessage, InfoBarSeverity.Success);
            ProfileCombo.SelectedItem = null;
        }
    }

    private void EnableAll_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.EnableAll();
        UpdateList();
        ShowStatus(_viewModel.StatusMessage, InfoBarSeverity.Success);
    }

    private void DisableAll_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.DisableAll();
        UpdateList();
        ShowStatus(_viewModel.StatusMessage, InfoBarSeverity.Success);
    }

    private void Enable_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            _viewModel.EnableSetting(id);
            UpdateList();
            var succeeded = _viewModel.StatusMessage.StartsWith("Enabled:", StringComparison.Ordinal);
            ShowStatus(_viewModel.StatusMessage, succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
        }
    }

    private void Disable_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            _viewModel.DisableSetting(id);
            UpdateList();
            var succeeded = _viewModel.StatusMessage.StartsWith("Disabled:", StringComparison.Ordinal);
            ShowStatus(_viewModel.StatusMessage, succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
        }
    }

    private void UpdateList()
    {
        SettingsList.ItemsSource = _viewModel.Settings;
    }

    private void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Success)
    {
        StatusBar.Message = message;
        StatusBar.Severity = severity;
        StatusBar.IsOpen = true;
    }
}
