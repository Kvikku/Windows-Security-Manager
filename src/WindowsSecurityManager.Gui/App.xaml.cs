using Microsoft.UI.Xaml;
using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Gui;

/// <summary>
/// Application entry point. Initializes core services and creates the main window.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The core security settings manager shared across the application.
    /// </summary>
    public static SecuritySettingsManager SettingsManager { get; private set; } = null!;

    /// <summary>
    /// The registry service instance shared across the application.
    /// </summary>
    public static IRegistryService RegistryService { get; private set; } = null!;

    /// <summary>
    /// The audit logger instance (may be null if initialization failed).
    /// </summary>
    public static AuditLogger? AuditLogger { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize core services (same bootstrap as CLI)
        RegistryService = new RegistryService();
        ISecuritySettingProvider[] providers =
        [
            new DefenderSettings(),
            new AsrSettings(),
            new FirewallSettings(),
            new CisBenchmarkSettings(),
            new AccountPolicySettings(),
            new NetworkSecuritySettings()
        ];

        SettingsManager = new SecuritySettingsManager(RegistryService, providers);

        // Set up audit logger
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsSecurityManager");
        var logPath = Path.Combine(logDir, "wsm-audit.log");
        try
        {
            AuditLogger = new AuditLogger(logPath);
            SettingsManager.SetAuditLogger(AuditLogger);
        }
        catch (UnauthorizedAccessException)
        {
            // Audit logging will be unavailable
        }
        catch (IOException)
        {
            // Audit logging will be unavailable
        }

        _window = new MainWindow();
        _window.Activate();
    }
}
