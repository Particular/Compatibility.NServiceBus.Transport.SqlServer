using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SQLServer;
using NServiceBus.Compatibility;

class Base : ITestBehavior
{
    public EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;

        var config = new EndpointConfiguration(opts.ApplyUniqueRunPrefix(endpointName));

        var transport = config.UseTransport<SqlServerTransport>();
        transport.ConnectionString(opts.ConnectionString + $";App={endpointName}");
        transport.Transactions(TransportTransactionMode.ReceiveOnly);
        transport.SubscriptionSettings().SubscriptionTableName(opts.ApplyUniqueRunPrefix("SubscriptionRouting"));

        Configure(opts, config, transport, transport.Routing());

        return config;
    }

    protected virtual void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
    }

    public virtual Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}