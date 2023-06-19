namespace NServiceBus.Compatibility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Customization;
using NServiceBus.Transport;

/// <summary>
/// Base class for wire compatibility test behaviors
/// </summary>
public abstract class Plugin : IPlugin
{
    IEndpointInstance instance;

    /// <summary>
    /// Starts the test endpoint.
    /// </summary>
    public async Task StartEndpoint(
        PluginOptions opts,
        CancellationToken cancellationToken = default)
    {
        var config = Configure(opts);
        config.EnableInstallers();
        config.PurgeOnStartup(true);

        config.UsePersistence<InMemoryPersistence>();

        config.Pipeline.Register(b => new StampVersionBehavior(b.Build<IDispatchMessages>()), "Stamps version");
        config.Pipeline.Register(new DiscardBehavior(opts.TestRunId), nameof(DiscardBehavior));

        config.Conventions().DefiningMessagesAs(t => t.GetInterfaces().Any(x => x.Name == "IMessage"));
        config.Conventions().DefiningCommandsAs(t => t.GetInterfaces().Any(x => x.Name == "ICommand"));
        config.Conventions().DefiningEventsAs(t => t.GetInterfaces().Any(x => x.Name == "IEvent"));

        config.SendFailedMessagesTo(opts.ApplyUniqueRunPrefix("error"));
        config.AuditProcessedMessagesTo(opts.AuditQueue);
        config.AddHeaderToAllOutgoingMessages(nameof(opts.TestRunId), opts.TestRunId);

        config.TypesToIncludeInScan(GetTypesToScan(GetType()).ToList());

        instance = await Endpoint.Start(config).ConfigureAwait(false);
    }

    IEnumerable<Type> GetTypesToScan(Type behaviorType)
    {
        yield return behaviorType;
        foreach (var nested in behaviorType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            yield return nested;
        }

        if (behaviorType.BaseType != null)
        {
            var baseTypes = GetTypesToScan(behaviorType.BaseType);
            foreach (Type type in baseTypes)
            {
                yield return type;
            }
        }
    }

    /// <summary>
    /// Invoked when the test is starting.
    /// </summary>
    public Task StartTest(CancellationToken cancellationToken = default) => Execute(instance, cancellationToken);

    /// <summary>
    /// Invoked when the test is stopping.
    /// </summary>
    /// <returns></returns>
    public Task Stop(CancellationToken cancellationToken = default) => instance.Stop();

    /// <summary>
    /// Override to provide test behavior logic.
    /// </summary>
    protected virtual Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Override to prepare the endpoint configuration.
    /// </summary>
    protected abstract EndpointConfiguration Configure(PluginOptions opts);
}