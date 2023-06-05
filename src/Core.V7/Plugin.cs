﻿namespace NServiceBus.Compatibility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Customization;
using NServiceBus.Transport;

public class Plugin : IPlugin
{
    IEndpointInstance instance;
    ITestBehavior behavior;

    public async Task StartEndpoint(
        string behaviorClassName,
        PluginOptions opts,
        CancellationToken cancellationToken = default)
    {
        var behaviorClass = Type.GetType(behaviorClassName, true);

        Console.Out.WriteLine($">> Creating {behaviorClass}");

        behavior = CreateBehavior(behaviorClass);

        var config = behavior.Configure(opts);
        config.EnableInstallers();
        config.PurgeOnStartup(true);

        config.UsePersistence<InMemoryPersistence>();

        config.Pipeline.Register(b => new StampVersionBehavior(b.Build<IDispatchMessages>()), "Stamps version");
        config.Pipeline.Register(new DiscardBehavior(opts.TestRunId), nameof(DiscardBehavior));

        config.Conventions().DefiningMessagesAs(t => t.GetInterfaces().Any(x => x.Name == "IMessage"));
        config.Conventions().DefiningCommandsAs(t => t.GetInterfaces().Any(x => x.Name == "ICommand"));
        config.Conventions().DefiningEventsAs(t => t.GetInterfaces().Any(x => x.Name == "IEvent"));

        config.AuditProcessedMessagesTo(opts.AuditQueue);
        config.AddHeaderToAllOutgoingMessages(nameof(opts.TestRunId), opts.TestRunId);

        config.TypesToIncludeInScan(GetTypesToScan(behaviorClass).ToList());

        instance = await Endpoint.Start(config).ConfigureAwait(false);
    }

    ITestBehavior CreateBehavior(Type behaviorClass)
    {
        try
        {
            return (ITestBehavior)Activator.CreateInstance(behaviorClass);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create behavior for {behaviorClass.FullName}", ex);
        }

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

    public Task StartTest(CancellationToken cancellationToken = default) => behavior.Execute(instance, cancellationToken);

    public Task Stop(CancellationToken cancellationToken = default) => instance.Stop();
}