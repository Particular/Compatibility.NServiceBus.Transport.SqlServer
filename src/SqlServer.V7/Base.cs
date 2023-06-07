using NServiceBus;
using NServiceBus.Compatibility;

abstract class Base : Plugin
{
    protected override EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;

        var config = new EndpointConfiguration(opts.ApplyUniqueRunPrefix(endpointName));

        var transport = new SqlServerTransport(opts.ConnectionStrings[endpointName])
        {
            //TransportTransactionMode = TransportTransactionMode.ReceiveOnly,
            TransportTransactionMode = TransportTransactionMode.SendsAtomicWithReceive,
        };

        transport.Subscriptions.SubscriptionTableName = new NServiceBus.Transport.SqlServer.SubscriptionTableName(opts.ApplyUniqueRunPrefix("SubscriptionRouting"));

        var routingConfig = config.UseTransport(transport);

        Configure(opts, config, transport, routingConfig);

        return config;
    }

    protected virtual void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
        )
    {
    }
}