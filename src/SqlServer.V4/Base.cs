using NServiceBus;
using NServiceBus.Compatibility;

abstract class Base : Plugin
{
    protected override EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;
        var config = new EndpointConfiguration(opts.ApplyUniqueRunPrefix(endpointName));

        var transport = config.UseTransport<SqlServerTransport>();
        transport.ConnectionString(opts.ConnectionStrings[endpointName]);
        transport.Transactions(TransportTransactionMode.ReceiveOnly);
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
}