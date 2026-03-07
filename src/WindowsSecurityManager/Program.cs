using System.CommandLine;
using WindowsSecurityManager.Commands;
using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Services;

var rootCommand = new RootCommand("Windows Security Manager - Enable, disable, and report on Windows security hardening settings");

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

// Register commands
rootCommand.AddCommand(EnableCommand.Create(manager));
rootCommand.AddCommand(DisableCommand.Create(manager));
rootCommand.AddCommand(ReportCommand.Create(manager));
rootCommand.AddCommand(ListCommand.Create(manager));

return await rootCommand.InvokeAsync(args);
