using Moq;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Tests;

/// <summary>
/// Shared test context for command-layer tests. Handles Console.Out redirection,
/// mock registry setup, and SecuritySettingsManager initialization.
/// </summary>
internal sealed class CommandTestContext : IDisposable
{
    private readonly TextWriter _originalOut;
    private readonly StringWriter _writer;

    public Mock<IRegistryService> MockRegistry { get; }
    public SecuritySettingsManager Manager { get; }

    public CommandTestContext()
    {
        MockRegistry = new Mock<IRegistryService>();
        Manager = new SecuritySettingsManager(MockRegistry.Object, new[] { new TestSettingProvider() });
        _originalOut = Console.Out;
        _writer = new StringWriter();
        Console.SetOut(_writer);
    }

    public string GetOutput() => _writer.ToString();

    public void Dispose()
    {
        Console.SetOut(_originalOut);
        _writer.Dispose();
    }
}
