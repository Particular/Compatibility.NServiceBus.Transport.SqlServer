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
        transportConfig.UseCatalogForQueue(opts.AuditQueue, "nservicebus");
        transportConfig.UseCatalogForQueue(opts.ApplyUniqueRunPrefix(nameof(CatalogReceiver)), "nservicebus1");
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

        transportConfig.UseCatalogForQueue(opts.AuditQueue, "nservicebus");
    }
}