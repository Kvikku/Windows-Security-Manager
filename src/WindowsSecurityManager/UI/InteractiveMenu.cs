using Spectre.Console;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.UI;

/// <summary>
/// Interactive terminal menu system using Spectre.Console.
/// </summary>
public class InteractiveMenu
{
    private readonly SecuritySettingsManager _manager;

    private const string MenuList = "List Settings";
    private const string MenuReport = "Compliance Report";
    private const string MenuEnable = "Enable Settings";
    private const string MenuDisable = "Disable Settings";
    private const string MenuExit = "Exit";

    public InteractiveMenu(SecuritySettingsManager manager)
    {
        _manager = manager;
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
        var version = new Markup("[dim]v1.0.0 — Security Hardening Made Simple[/]").Centered();

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

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]What would you like to do?[/]")
                .PageSize(8)
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .AddChoices(MenuList, MenuReport, MenuEnable, MenuDisable, MenuExit));

        return choice;
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
                .AddColumn(new TableColumn("[bold]Type[/]").Centered().Width(8))
                .AddColumn(new TableColumn("[bold]Enable → Disable[/]").Centered().Width(18));

            foreach (var s in group)
            {
                table.AddRow(
                    $"[cyan]{s.Id}[/]",
                    Markup.Escape(s.Name),
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

        var summaryGrid = new Grid()
            .AddColumn()
            .AddColumn();

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
                .AddChoices("A specific setting", "An entire category", "All settings", "← Back"));

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
        }
        else
        {
            var setting = PromptSetting();
            if (setting == null) return;

            if (!ConfirmAction($"[yellow]Enable '{setting.Name}'?[/]"))
                return;

            bool success = _manager.EnableSetting(setting.Id);
            if (success)
                AnsiConsole.MarkupLine($"[green]✓ '{setting.Name}' enabled successfully.[/]");
            else
                AnsiConsole.MarkupLine($"[red]✗ Failed to enable '{setting.Name}'.[/]");
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
                .AddChoices("A specific setting", "An entire category", "All settings", "← Back"));

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
        }
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
