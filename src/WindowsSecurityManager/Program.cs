using System.CommandLine;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Services;
using WindowsSecurityManager.UI;

// Create the registry service and setting providers
IRegistryService registryService = new RegistryService();
ISecuritySettingProvider[] providers =
[
    new DefenderSettings(),
    new AsrSettings(),
    new FirewallSettings(),
    new CisBenchmarkSettings(),
    new AccountPolicySettings(),
    new NetworkSecuritySettings()
];

var manager = new SecuritySettingsManager(registryService, providers);

// Set up audit logger
var logPath = Path.Combine(AppContext.BaseDirectory, "wsm-audit.log");
var auditLogger = new AuditLogger(logPath);
manager.SetAuditLogger(auditLogger);

// If no arguments provided, launch interactive mode
if (args.Length == 0)
{
    var menu = new InteractiveMenu(manager, registryService, auditLogger);
    menu.Run();
    return 0;
}

// Otherwise, use the CLI command interface
var rootCommand = new RootCommand("Windows Security Manager - Enable, disable, and report on Windows security hardening settings");

rootCommand.AddCommand(EnableCommand.Create(manager));
rootCommand.AddCommand(DisableCommand.Create(manager));
rootCommand.AddCommand(ReportCommand.Create(manager));
rootCommand.AddCommand(ListCommand.Create(manager));
rootCommand.AddCommand(DetailCommand.Create(manager));
rootCommand.AddCommand(ProfileCommand.Create(manager));
rootCommand.AddCommand(BackupCommand.Create(manager, registryService, auditLogger));
rootCommand.AddCommand(RestoreCommand.Create(manager, registryService, auditLogger));

return await rootCommand.InvokeAsync(args);
