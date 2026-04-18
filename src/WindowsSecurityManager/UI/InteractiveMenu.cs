using System.Reflection;
using Spectre.Console;
using Spectre.Console.Rendering;
using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.UI;

/// <summary>
/// Interactive terminal menu system using Spectre.Console.
/// Features: live dashboard, search, profiles, multi-select, export, backup/restore, auto-refresh.
/// </summary>
public class InteractiveMenu
{
    private readonly SecuritySettingsManager _manager;
    private readonly IRegistryService _registryService;
    private readonly AuditLogger? _auditLogger;

    private const string MenuList = "📋 List Settings";
    private const string MenuReport = "📊 Compliance Report";
    private const string MenuEnable = "🟢 Enable Settings";
    private const string MenuDisable = "🔴 Disable Settings";
    private const string MenuSearch = "🔍 Search Settings";
    private const string MenuDetail = "🔎 Setting Detail";
    private const string MenuProfiles = "⚡ Security Profiles";
    private const string MenuExport = "💾 Export Report";
    private const string MenuBackup = "📦 Backup / Restore";
    private const string MenuExit = "🚪 Exit";

    public InteractiveMenu(SecuritySettingsManager manager, IRegistryService registryService, AuditLogger? auditLogger = null)
    {
        _manager = manager;
        _registryService = registryService;
        _auditLogger = auditLogger;
    }

    public void Run()
    {
        ShowIntro();

        while (true)
        {
            var choice = ShowMainMenu();
            if (choice == MenuExit)
            {
                ShowExitMessage();
                break;
            }

            AnsiConsole.Clear();
            switch (choice)
            {
                case MenuList:
                    ShowListView();
                    break;
                case MenuReport:
                    ShowReportView();
                    break;
                case MenuEnable:
                    ShowEnableView();
                    break;
                case MenuDisable:
                    ShowDisableView();
                    break;
                case MenuSearch:
                    ShowSearchView();
                    break;
                case MenuDetail:
                    ShowDetailView();
                    break;
                case MenuProfiles:
                    ShowProfilesView();
                    break;
                case MenuExport:
                    ShowExportView();
                    break;
                case MenuBackup:
                    ShowBackupRestoreView();
                    break;
            }

            WaitForKey();
        }
    }

    private void ShowIntro()
    {
        AnsiConsole.Clear();

        var ascii = new FigletText("WSM")
            .Centered()
            .Color(Color.Cyan1);

        var subtitle = new Markup("[bold cyan]Windows Security Manager[/]").Centered();
        var appVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion.Split('+')[0] ?? "1.0.0";
        var version = new Markup($"[dim]v{appVersion} — Security Hardening Made Simple[/]").Centered();

        var panel = new Panel(
            new Rows(
                new Text(""),
                ascii,
                subtitle,
                new Text(""),
                version,
                new Text(""),
                new Markup("[dim grey]Enable, disable, and audit Windows security settings[/]").Centered(),
                new Markup("[dim grey]aligned with CIS Benchmarks and Microsoft best practices[/]").Centered(),
                new Text("")
            ))
        {
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Cyan1),
            Padding = new Padding(2, 0)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Quick system summary
        var report = _manager.GenerateReport();
        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        summaryTable.AddRow("[dim]Total Settings[/]", $"[white]{report.TotalSettings}[/]");
        summaryTable.AddRow("[green]Hardened[/]", $"[green]{report.EnabledCount}[/]");
        summaryTable.AddRow("[red]Not Hardened[/]", $"[red]{report.DisabledCount}[/]");

        var compColor = report.CompliancePercentage switch
        {
            >= 80 => "green",
            >= 50 => "yellow",
            _ => "red"
        };
        summaryTable.AddRow("[bold]Compliance[/]", $"[bold {compColor}]{report.CompliancePercentage:F1}%[/]");

        AnsiConsole.Write(Align.Center(summaryTable));
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Markup("[dim]Press any key to continue...[/]").Centered());
        Console.ReadKey(true);
    }

    private string ShowMainMenu()
    {
        AnsiConsole.Clear();

        var title = new Rule("[bold cyan]Windows Security Manager[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        };
        AnsiConsole.Write(title);
        AnsiConsole.WriteLine();

        // Live dashboard — per-category compliance bars
        ShowDashboard();
        AnsiConsole.WriteLine();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to do?[/]")
                .PageSize(12)
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .AddChoices(
                    MenuList, MenuReport, MenuEnable, MenuDisable,
                    MenuSearch, MenuDetail, MenuProfiles,
                    MenuExport, MenuBackup, MenuExit));

        return choice;
    }

    /// <summary>
    /// Live dashboard showing per-category compliance bars.
    /// </summary>
    private void ShowDashboard()
    {
        var report = _manager.GenerateReport();
        var grouped = report.Settings
            .GroupBy(s => s.Setting.Category)
            .OrderBy(g => g.Key.ToString());

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[bold dim]Security Posture[/]")
            .AddColumn(new TableColumn("[bold]Category[/]").Width(25))
            .AddColumn(new TableColumn("[bold]Compliance[/]").Width(40))
            .AddColumn(new TableColumn("[bold]Score[/]").Centered().Width(12));

        foreach (var group in grouped)
        {
            int enabled = group.Count(s => s.IsEnabled);
            int total = group.Count();
            double pct = total > 0 ? (double)enabled / total * 100 : 0;

            var barColor = pct >= 80 ? "green" : pct >= 50 ? "yellow" : "red";
            int filled = (int)(pct / 100 * 20);
            string bar = new string('█', filled) + new string('░', 20 - filled);

            table.AddRow(
                $"[cyan]{group.Key}[/]",
                $"[{barColor}]{bar}[/]",
                $"[{barColor}]{enabled}/{total}[/]"
            );
        }

        // Overall row
        var overallColor = report.CompliancePercentage >= 80 ? "green" : report.CompliancePercentage >= 50 ? "yellow" : "red";
        int overallFilled = (int)(report.CompliancePercentage / 100 * 20);
        string overallBar = new string('█', overallFilled) + new string('░', 20 - overallFilled);
        table.AddEmptyRow();
        table.AddRow(
            $"[bold]Overall[/]",
            $"[bold {overallColor}]{overallBar}[/]",
            $"[bold {overallColor}]{report.CompliancePercentage:F0}%[/]"
        );

        AnsiConsole.Write(table);
    }

    private void ShowListView()
    {
        var category = PromptCategoryOrAll("list");
        var settings = _manager.GetSettings(category);

        AnsiConsole.Write(new Rule($"[bold cyan]Available Settings{(category.HasValue ? $" — {category.Value}" : "")}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        var grouped = settings
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Key.ToString());

        foreach (var group in grouped)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title($"[bold yellow]{group.Key}[/] [dim]({group.Count()} settings)[/]")
                .AddColumn(new TableColumn("[bold]ID[/]").Width(10))
                .AddColumn(new TableColumn("[bold]Name[/]").Width(45))
                .AddColumn(new TableColumn("[bold]Impact[/]").Centered().Width(10))
                .AddColumn(new TableColumn("[bold]Type[/]").Centered().Width(8))
                .AddColumn(new TableColumn("[bold]Enable → Disable[/]").Centered().Width(18));

            foreach (var s in group)
            {
                table.AddRow(
                    $"[cyan]{s.Id}[/]",
                    Markup.Escape(s.Name),
                    FormatImpactMarkup(s.Impact),
                    $"[dim]{s.ValueType}[/]",
                    $"[green]{s.EnabledValue}[/] → [red]{s.DisabledValue}[/]"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        AnsiConsole.MarkupLine($"[dim]Total: {settings.Count} settings[/]");
    }

    private void ShowReportView()
    {
        var category = PromptCategoryOrAll("report on");

        SecurityReport report = null!;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start("[cyan]Scanning registry...[/]", ctx =>
            {
                report = _manager.GenerateReport(category);
            });

        AnsiConsole.Write(new Rule($"[bold cyan]Compliance Report{(category.HasValue ? $" — {category.Value}" : "")}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        // Summary panel
        var compColor = report.CompliancePercentage switch
        {
            >= 80 => "green",
            >= 50 => "yellow",
            _ => "red"
        };

        var chart = new BreakdownChart()
            .Width(60)
            .AddItem("Hardened", report.EnabledCount, Color.Green)
            .AddItem("Misconfigured", report.DisabledCount - report.NotConfiguredCount, Color.Yellow)
            .AddItem("Missing", report.NotConfiguredCount, Color.Red);

        var statsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .HideHeaders()
            .AddColumn("Metric")
            .AddColumn("Value");

        statsTable.AddRow("[dim]Generated[/]", $"[white]{report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC[/]");
        statsTable.AddRow("[dim]Total Settings[/]", $"[white]{report.TotalSettings}[/]");
        statsTable.AddRow("[green]Hardened[/]", $"[green]{report.EnabledCount}[/]");
        statsTable.AddRow("[yellow]Misconfigured[/]", $"[yellow]{report.DisabledCount - report.NotConfiguredCount}[/]");
        statsTable.AddRow("[red]Not Configured[/]", $"[red]{report.NotConfiguredCount}[/]");
        statsTable.AddRow("[bold]Compliance[/]", $"[bold {compColor}]{report.CompliancePercentage:F1}%[/]");

        AnsiConsole.Write(statsTable);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(chart);
        AnsiConsole.WriteLine();

        // Per-category details
        var grouped = report.Settings
            .GroupBy(s => s.Setting.Category)
            .OrderBy(g => g.Key.ToString());

        foreach (var group in grouped)
        {
            int enabled = group.Count(s => s.IsEnabled);
            int total = group.Count();
            var catColor = enabled == total ? "green" : enabled > 0 ? "yellow" : "red";

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title($"[bold yellow]{group.Key}[/] [{catColor}]({enabled}/{total} hardened)[/]")
                .AddColumn(new TableColumn("[bold]Status[/]").Centered().Width(8))
                .AddColumn(new TableColumn("[bold]ID[/]").Width(10))
                .AddColumn(new TableColumn("[bold]Name[/]").Width(40))
                .AddColumn(new TableColumn("[bold]Current[/]").Centered().Width(10))
                .AddColumn(new TableColumn("[bold]Expected[/]").Centered().Width(10));

            foreach (var status in group)
            {
                string icon = status.IsEnabled ? "[green]✓[/]"
                    : status.IsConfigured ? "[yellow]✗[/]"
                    : "[red]—[/]";

                string state = status.IsEnabled ? "[green]OK[/]"
                    : status.IsConfigured ? "[yellow]WRONG[/]"
                    : "[red]MISSING[/]";

                string curVal = status.CurrentValue?.ToString() ?? "[dim]N/A[/]";

                table.AddRow(
                    $"{icon} {state}",
                    $"[cyan]{status.Setting.Id}[/]",
                    Markup.Escape(status.Setting.Name),
                    curVal,
                    $"[dim]{status.Setting.EnabledValue}[/]"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }

    private void ShowEnableView()
    {
        AnsiConsole.Write(new Rule("[bold green]Enable (Harden) Settings[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("green")
        });
        AnsiConsole.WriteLine();

        var scope = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to enable?[/]")
                .HighlightStyle(new Style(Color.Green))
                .AddChoices("A specific setting", "An entire category", "Multiple settings (multi-select)", "All settings", "← Back"));

        if (scope == "← Back") return;

        if (scope == "All settings")
        {
            if (!ConfirmAction("[yellow]Enable ALL security settings?[/] This will modify the registry."))
                return;

            int count = 0;
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start("[green]Enabling all settings...[/]", ctx =>
                {
                    count = _manager.EnableAll();
                });

            AnsiConsole.MarkupLine($"[green]✓ Enabled {count} security settings.[/]");
            ShowAutoRefresh();
        }
        else if (scope == "An entire category")
        {
            var category = PromptCategory();
            if (!ConfirmAction($"[yellow]Enable all settings in '{category}'?[/]"))
                return;

            int count = 0;
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start($"[green]Enabling {category} settings...[/]", ctx =>
                {
                    count = _manager.EnableCategory(category);
                });

            AnsiConsole.MarkupLine($"[green]✓ Enabled {count} settings in '{category}'.[/]");
            ShowAutoRefresh(category);
        }
        else if (scope == "Multiple settings (multi-select)")
        {
            ShowMultiSelectEnable();
        }
        else
        {
            var setting = PromptSetting();
            if (setting == null) return;

            string impactInfo = setting.Impact == ImpactLevel.Unknown
                ? string.Empty
                : $" — Impact: {FormatImpactMarkup(setting.Impact)}";
            if (!string.IsNullOrWhiteSpace(setting.Consequences))
            {
                AnsiConsole.MarkupLine($"[yellow]⚠  {Markup.Escape(setting.Consequences)}[/]");
            }
            if (!ConfirmAction($"[yellow]Enable '{Markup.Escape(setting.Name)}'?[/]{impactInfo}"))
                return;

            bool success = _manager.EnableSetting(setting.Id);
            if (success)
                AnsiConsole.MarkupLine($"[green]✓ '{Markup.Escape(setting.Name)}' enabled successfully.[/]");
            else
                AnsiConsole.MarkupLine($"[red]✗ Failed to enable '{Markup.Escape(setting.Name)}'.[/]");

            ShowAutoRefreshSingle(setting);
        }
    }

    private void ShowDisableView()
    {
        AnsiConsole.Write(new Rule("[bold red]Disable (Unharden) Settings[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("red")
        });
        AnsiConsole.WriteLine();

        var scope = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to disable?[/]")
                .HighlightStyle(new Style(Color.Red))
                .AddChoices("A specific setting", "An entire category", "Multiple settings (multi-select)", "All settings", "← Back"));

        if (scope == "← Back") return;

        if (scope == "All settings")
        {
            if (!ConfirmAction("[red bold]Disable ALL security settings?[/] This will weaken system security."))
                return;

            int count = 0;
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("red"))
                .Start("[red]Disabling all settings...[/]", ctx =>
                {
                    count = _manager.DisableAll();
                });

            AnsiConsole.MarkupLine($"[red]✗ Disabled {count} security settings.[/]");
            ShowAutoRefresh();
        }
        else if (scope == "An entire category")
        {
            var category = PromptCategory();
            if (!ConfirmAction($"[red]Disable all settings in '{category}'?[/]"))
                return;

            int count = 0;
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("red"))
                .Start($"[red]Disabling {category} settings...[/]", ctx =>
                {
                    count = _manager.DisableCategory(category);
                });

            AnsiConsole.MarkupLine($"[red]✗ Disabled {count} settings in '{category}'.[/]");
            ShowAutoRefresh(category);
        }
        else if (scope == "Multiple settings (multi-select)")
        {
            ShowMultiSelectDisable();
        }
        else
        {
            var setting = PromptSetting();
            if (setting == null) return;

            if (!ConfirmAction($"[red]Disable '{setting.Name}'?[/]"))
                return;

            bool success = _manager.DisableSetting(setting.Id);
            if (success)
                AnsiConsole.MarkupLine($"[red]✗ '{setting.Name}' disabled.[/]");
            else
                AnsiConsole.MarkupLine($"[red]Failed to disable '{setting.Name}'.[/]");

            ShowAutoRefreshSingle(setting);
        }
    }

    /// <summary>
    /// Multi-select enable: pick multiple settings using checkboxes.
    /// </summary>
    private void ShowMultiSelectEnable()
    {
        var category = PromptCategory();
        var settings = _manager.GetSettings(category);

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"[bold green]Select settings to enable in {category}:[/]")
                .PageSize(20)
                .HighlightStyle(new Style(Color.Green))
                .InstructionsText("[grey](Press [green]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
                .AddChoices(settings.Select(s => $"[{s.Id}] {s.Name}")));

        if (selected.Count == 0) return;

        var ids = selected.Select(s => s.Split(']')[0].TrimStart('[')).ToList();

        if (!ConfirmAction($"[yellow]Enable {ids.Count} selected settings?[/]"))
            return;

        int count = _manager.EnableSettings(ids);
        AnsiConsole.MarkupLine($"[green]✓ Enabled {count} settings.[/]");
        ShowAutoRefresh(category);
    }

    /// <summary>
    /// Multi-select disable: pick multiple settings using checkboxes.
    /// </summary>
    private void ShowMultiSelectDisable()
    {
        var category = PromptCategory();
        var settings = _manager.GetSettings(category);

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"[bold red]Select settings to disable in {category}:[/]")
                .PageSize(20)
                .HighlightStyle(new Style(Color.Red))
                .InstructionsText("[grey](Press [red]<space>[/] to toggle, [red]<enter>[/] to accept)[/]")
                .AddChoices(settings.Select(s => $"[{s.Id}] {s.Name}")));

        if (selected.Count == 0) return;

        var ids = selected.Select(s => s.Split(']')[0].TrimStart('[')).ToList();

        if (!ConfirmAction($"[red]Disable {ids.Count} selected settings?[/]"))
            return;

        int count = _manager.DisableSettings(ids);
        AnsiConsole.MarkupLine($"[red]✗ Disabled {count} settings.[/]");
        ShowAutoRefresh(category);
    }

    /// <summary>
    /// Search settings by keyword.
    /// </summary>
    private void ShowSearchView()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Search Settings[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        var keyword = AnsiConsole.Ask<string>("[bold]Enter search keyword:[/]");
        var results = _manager.SearchSettings(keyword);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No settings found matching '{Markup.Escape(keyword)}'.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title($"[bold cyan]Search Results for '{Markup.Escape(keyword)}'[/] [dim]({results.Count} matches)[/]")
            .AddColumn(new TableColumn("[bold]ID[/]").Width(10))
            .AddColumn(new TableColumn("[bold]Name[/]").Width(40))
            .AddColumn(new TableColumn("[bold]Category[/]").Width(22))
            .AddColumn(new TableColumn("[bold]Type[/]").Centered().Width(8));

        foreach (var s in results)
        {
            table.AddRow(
                $"[cyan]{s.Id}[/]",
                Markup.Escape(s.Name),
                $"[dim]{s.Category}[/]",
                $"[dim]{s.ValueType}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Detailed view of a single setting.
    /// </summary>
    private void ShowDetailView()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Setting Detail[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        var setting = PromptSetting();
        if (setting == null) return;

        var status = _manager.GetSettingStatus(setting);

        var rows = new List<IRenderable>
        {
            new Markup($"[bold cyan]ID:[/]            {Markup.Escape(setting.Id)}"),
            new Markup($"[bold cyan]Name:[/]          {Markup.Escape(setting.Name)}"),
            new Markup($"[bold cyan]Description:[/]   {Markup.Escape(setting.Description)}"),
            new Markup($"[bold cyan]Category:[/]      {setting.Category}"),
            new Markup($"[bold cyan]Impact:[/]        {FormatImpactMarkup(setting.Impact)}"),
        };
        if (!string.IsNullOrWhiteSpace(setting.Consequences))
        {
            rows.Add(new Markup($"[bold cyan]Consequences:[/] [yellow]{Markup.Escape(setting.Consequences)}[/]"));
        }
        rows.AddRange(new IRenderable[]
        {
            new Text(""),
            new Markup("[bold yellow]Registry:[/]"),
            new Markup($"  [dim]Hive:[/]         {Markup.Escape(setting.RegistryHive)}"),
            new Markup($"  [dim]Path:[/]         {Markup.Escape(setting.RegistryPath)}"),
            new Markup($"  [dim]Value Name:[/]   {Markup.Escape(setting.ValueName)}"),
            new Markup($"  [dim]Value Type:[/]   {setting.ValueType}"),
            new Text(""),
            new Markup("[bold yellow]Values:[/]"),
            new Markup($"  [green]Enabled:[/]      {setting.EnabledValue}"),
            new Markup($"  [red]Disabled:[/]     {setting.DisabledValue}"),
            new Markup($"  [blue]Recommended:[/]  {setting.RecommendedValue}"),
            new Text(""),
            new Markup("[bold yellow]Current Status:[/]"),
            new Markup($"  [dim]State:[/]         {(status.IsEnabled ? "[green]✓ ENABLED[/]" : status.IsConfigured ? "[yellow]✗ DISABLED[/]" : "[red]? MISSING[/]")}"),
            new Markup($"  [dim]Current Value:[/]  {Markup.Escape(status.CurrentValue?.ToString() ?? "N/A (not configured)")}"),
            new Markup($"  [dim]Is Configured:[/]  {status.IsConfigured}"),
            new Markup($"  [dim]Matches Recommended:[/] {status.MatchesRecommended}")
        });

        var panel = new Panel(new Rows(rows))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1),
            Header = new PanelHeader($"[bold] {Markup.Escape(setting.Name)} [/]"),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Returns a Spectre.Console-friendly markup string for an <see cref="ImpactLevel"/>.
    /// </summary>
    private static string FormatImpactMarkup(ImpactLevel level) => level switch
    {
        ImpactLevel.Low => "[green]🟢 Low[/]",
        ImpactLevel.Medium => "[yellow]🟡 Medium[/]",
        ImpactLevel.High => "[red]🔴 High[/]",
        _ => "[grey]⚪ Unknown[/]"
    };

    /// <summary>
    /// Security profiles view — list and apply presets.
    /// </summary>
    private void ShowProfilesView()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Security Profiles[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        var profiles = SecurityProfiles.GetProfiles();
        var choices = profiles.Select(p => $"{p.Name} ({p.SettingIds.Count} settings)").ToList();
        choices.Add("← Back");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select a profile to view or apply:[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .AddChoices(choices));

        if (choice == "← Back") return;

        var profileName = choice.Split(" (")[0];
        var profile = profiles.First(p => p.Name == profileName);

        // Show profile details (with impact summary)
        var profileSettings = _manager.GetSettings()
            .Where(s => profile.SettingIds.Contains(s.Id, StringComparer.OrdinalIgnoreCase))
            .ToList();
        int profileHigh = profileSettings.Count(s => s.Impact == ImpactLevel.High);
        int profileMedium = profileSettings.Count(s => s.Impact == ImpactLevel.Medium);
        int profileLow = profileSettings.Count(s => s.Impact == ImpactLevel.Low);

        AnsiConsole.MarkupLine($"[bold cyan]{Markup.Escape(profile.Name)}[/]");
        AnsiConsole.MarkupLine($"[dim]{Markup.Escape(profile.Description)}[/]");
        AnsiConsole.MarkupLine($"[dim]Settings: {profile.SettingIds.Count}[/]");
        AnsiConsole.MarkupLine($"Impact: [red]🔴 {profileHigh} High[/] | [yellow]🟡 {profileMedium} Medium[/] | [green]🟢 {profileLow} Low[/]");
        if (profileHigh > 0 || profileMedium > 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠  Contains Medium/High-impact settings — preview with dry-run and back up first.[/]");
        }
        AnsiConsole.WriteLine();

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to do?[/]")
                .AddChoices("Apply this profile", "Preview (dry run)", "← Back"));

        if (action == "← Back") return;

        if (action == "Preview (dry run)")
        {
            var settings = _manager.GetSettings()
                .Where(s => profile.SettingIds.Contains(s.Id, StringComparer.OrdinalIgnoreCase))
                .ToList();
            var changes = _manager.DryRunEnable(settings);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title($"[bold yellow]Dry Run — {Markup.Escape(profile.Name)}[/]")
                .AddColumn(new TableColumn("[bold]ID[/]").Width(10))
                .AddColumn(new TableColumn("[bold]Name[/]").Width(34))
                .AddColumn(new TableColumn("[bold]Impact[/]").Centered().Width(10))
                .AddColumn(new TableColumn("[bold]Current[/]").Centered().Width(9))
                .AddColumn(new TableColumn("[bold]→ New[/]").Centered().Width(9));

            foreach (var change in changes)
            {
                string current = change.IsCurrentlyConfigured ? change.CurrentValue?.ToString() ?? "N/A" : "[red]NOT SET[/]";
                table.AddRow(
                    $"[cyan]{change.Setting.Id}[/]",
                    Markup.Escape(change.Setting.Name),
                    FormatImpactMarkup(change.Setting.Impact),
                    current,
                    $"[green]{change.NewValue}[/]"
                );
            }

            AnsiConsole.Write(table);

            // Surface High/Medium-impact consequences inline so the user sees side
            // effects before they choose to apply.
            var risky = changes
                .Where(c => c.Setting.Impact == ImpactLevel.High || c.Setting.Impact == ImpactLevel.Medium)
                .Where(c => !string.IsNullOrWhiteSpace(c.Setting.Consequences))
                .ToList();
            if (risky.Count > 0)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[bold yellow]⚠  Notable consequences:[/]");
                foreach (var c in risky)
                {
                    AnsiConsole.MarkupLine($"  {FormatImpactMarkup(c.Setting.Impact)} [cyan]{c.Setting.Id}[/] — [yellow]{Markup.Escape(c.Setting.Consequences)}[/]");
                }
            }
        }
        else
        {
            string confirmMsg = $"[yellow]Apply profile '{Markup.Escape(profile.Name)}'? This will enable {profile.SettingIds.Count} settings ([red]🔴 {profileHigh} High[/], [yellow]🟡 {profileMedium} Medium[/], [green]🟢 {profileLow} Low[/] impact).[/]";
            if (!ConfirmAction(confirmMsg))
                return;

            int count = _manager.EnableSettings(profile.SettingIds);
            AnsiConsole.MarkupLine($"[green]✓ Applied '{Markup.Escape(profile.Name)}': enabled {count} settings.[/]");
            if (profileHigh > 0 || profileMedium > 0)
            {
                AnsiConsole.MarkupLine("[dim]If something stops working, restore from your most recent backup. See docs/security-setting-consequences.md.[/]");
            }
            ShowAutoRefresh();
        }
    }

    /// <summary>
    /// Export report to file.
    /// </summary>
    private void ShowExportView()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Export Report[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        var formatChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select export format:[/]")
                .AddChoices("JSON", "CSV", "HTML", "← Back"));

        if (formatChoice == "← Back") return;

        var format = formatChoice switch
        {
            "JSON" => ExportFormat.Json,
            "CSV" => ExportFormat.Csv,
            "HTML" => ExportFormat.Html,
            _ => ExportFormat.Json
        };

        var extension = format switch
        {
            ExportFormat.Json => "json",
            ExportFormat.Csv => "csv",
            ExportFormat.Html => "html",
            _ => "json"
        };

        var defaultPath = $"wsm-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}";
        var filePath = AnsiConsole.Ask("[bold]Export file path:[/]", defaultPath);

        SecurityReport report = null!;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start("[cyan]Generating report...[/]", ctx =>
            {
                report = _manager.GenerateReport();
            });

        var exporter = new ReportExporter();
        exporter.ExportToFile(report, format, filePath);

        AnsiConsole.MarkupLine($"[green]✓ Report exported to '{Markup.Escape(filePath)}' ({formatChoice} format).[/]");
    }

    /// <summary>
    /// Backup/restore menu.
    /// </summary>
    private void ShowBackupRestoreView()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Backup / Restore[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to do?[/]")
                .AddChoices("Create backup", "Restore from backup", "← Back"));

        if (action == "← Back") return;

        if (action == "Create backup")
        {
            var defaultPath = $"wsm-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            var filePath = AnsiConsole.Ask("[bold]Backup file path:[/]", defaultPath);

            var backupService = new BackupService(_manager, _registryService);

            BackupData backup = null!;
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .Start("[cyan]Creating backup...[/]", ctx =>
                {
                    backup = backupService.CreateBackup();
                    backupService.SaveToFile(backup, filePath);
                });

            AnsiConsole.MarkupLine($"[green]✓ Backup saved to '{Markup.Escape(filePath)}' ({backup.Entries.Count} settings).[/]");
            _auditLogger?.Log("Backup", filePath, $"Backed up {backup.Entries.Count} settings");
        }
        else
        {
            var filePath = AnsiConsole.Ask<string>("[bold]Path to backup file:[/]");

            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]File not found: {Markup.Escape(filePath)}[/]");
                return;
            }

            if (!ConfirmAction("[yellow]Restore from backup? This will overwrite current registry values.[/]"))
                return;

            var backupService = new BackupService(_manager, _registryService);

            int count = 0;
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .Start("[cyan]Restoring...[/]", ctx =>
                {
                    count = backupService.RestoreFromFile(filePath);
                });

            AnsiConsole.MarkupLine($"[green]✓ Restored {count} settings from '{Markup.Escape(filePath)}'.[/]");
            _auditLogger?.Log("Restore", filePath, $"Restored {count} settings");
            ShowAutoRefresh();
        }
    }

    /// <summary>
    /// Auto-refresh: show updated compliance status after changes.
    /// </summary>
    private void ShowAutoRefresh(SecurityCategory? category = null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[dim]Updated Status[/]") { Style = Style.Parse("dim grey") });
        AnsiConsole.WriteLine();

        var report = _manager.GenerateReport(category);
        var compColor = report.CompliancePercentage >= 80 ? "green" : report.CompliancePercentage >= 50 ? "yellow" : "red";

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .HideHeaders()
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("[dim]Hardened[/]", $"[green]{report.EnabledCount}[/] / [white]{report.TotalSettings}[/]");
        table.AddRow("[bold]Compliance[/]", $"[bold {compColor}]{report.CompliancePercentage:F1}%[/]");

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Auto-refresh for a single setting.
    /// </summary>
    private void ShowAutoRefreshSingle(SecuritySetting setting)
    {
        AnsiConsole.WriteLine();
        var status = _manager.GetSettingStatus(setting);
        string stateIcon = status.IsEnabled ? "[green]✓ ENABLED[/]" : status.IsConfigured ? "[yellow]✗ DISABLED[/]" : "[red]? MISSING[/]";
        string currentVal = status.CurrentValue?.ToString() ?? "N/A";
        AnsiConsole.MarkupLine($"  [dim]Current state:[/] {stateIcon} [dim](value: {Markup.Escape(currentVal)})[/]");
    }

    private SecurityCategory? PromptCategoryOrAll(string action)
    {
        var choices = new List<string> { "All categories" };
        choices.AddRange(Enum.GetNames<SecurityCategory>());

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold]Select a category to {action}:[/]")
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .AddChoices(choices));

        if (choice == "All categories")
            return null;

        return Enum.Parse<SecurityCategory>(choice);
    }

    private SecurityCategory PromptCategory()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select a category:[/]")
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .AddChoices(Enum.GetNames<SecurityCategory>()));

        return Enum.Parse<SecurityCategory>(choice);
    }

    private SecuritySetting? PromptSetting()
    {
        var category = PromptCategory();
        var settings = _manager.GetSettings(category);

        var choices = settings.Select(s => $"[{s.Id}] {s.Name}").ToList();
        choices.Add("← Back");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold]Select a setting from {category}:[/]")
                .PageSize(20)
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .AddChoices(choices));

        if (choice == "← Back") return null;

        var id = choice.Split(']')[0].TrimStart('[');
        return settings.FirstOrDefault(s => s.Id == id);
    }

    private static bool ConfirmAction(string message)
    {
        return AnsiConsole.Confirm(message, defaultValue: false);
    }

    private static void WaitForKey()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[dim]Press any key to return to menu[/]") { Style = Style.Parse("dim grey") });
        Console.ReadKey(true);
    }

    private static void ShowExitMessage()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold cyan]Goodbye![/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("cyan")
        });
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Stay secure. ✦[/]");
        AnsiConsole.WriteLine();
    }
}
