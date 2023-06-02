using System.Threading.Tasks;
using System.Threading;
using NServiceBus;
using NServiceBus.Compatibility;

abstract class Base : ITestBehavior
{
    public EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;

        var config = new EndpointConfiguration(opts.ApplyUniqueRunPrefix(endpointName));

        var transport = config.UseTransport<SqlServerTransport>()
            .ConnectionString(opts.ConnectionString + $";App={endpointName}")
            .Transactions(TransportTransactionMode.ReceiveOnly);

        transport.SubscriptionSettings().SubscriptionTableName(opts.ApplyUniqueRunPrefix("SubscriptionRouting"));

        Configure(opts, config, transport);

        return config;
    }

    public virtual void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
        )
    { }

    public virtual Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}