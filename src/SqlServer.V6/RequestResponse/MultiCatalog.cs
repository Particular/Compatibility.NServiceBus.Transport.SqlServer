using NServiceBus;
using NServiceBus.Compatibility;

class CatalogSender : Sender
{
    public override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
    )
    {
        transportConfig.UseCatalogForQueue(opts.AuditQueue, MultiCatalogMap.Audit);
        transportConfig.UseCatalogForQueue(opts.ApplyUniqueRunPrefix(nameof(CatalogReceiver)), MultiCatalogMap.Receiver);
        var routingConfig = transportConfig.Routing();

        routingConfig.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(CatalogReceiver)));
    }
}

class CatalogReceiver : Receiver
{
    public override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
    )
    {
        base.Configure(opts, endpointConfig, transportConfig);

        transportConfig.UseCatalogForQueue(opts.AuditQueue, MultiCatalogMap.Audit);
    }
}