namespace NServiceBus.Compatibility;

/// <summary>
/// Defines the interface for the wire compatibility test behavior
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Starts the test endpoint.
    /// </summary>
    Task StartEndpoint(
        PluginOptions opts,
        CancellationToken cancellationToken = default
        );

    /// <summary>
    /// Invoked when the test is starting.
    /// </summary>
    Task StartTest(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked when the test is stopping.
    /// </summary>
    /// <returns></returns>
    Task Stop(CancellationToken cancellationToken = default);
}