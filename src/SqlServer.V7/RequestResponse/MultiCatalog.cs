using NServiceBus;
using NServiceBus.Compatibility;

class CatalogSender : Sender
{
    protected override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
        transportConfig.SchemaAndCatalog.UseCatalogForQueue(opts.AuditQueue, "nservicebus");
        transportConfig.SchemaAndCatalog.UseCatalogForQueue(opts.ApplyUniqueRunPrefix(nameof(CatalogReceiver)), "nservicebus1");

        routingConfig.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(CatalogReceiver)));
    }
}

class CatalogReceiver : Receiver
{
    protected override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
        base.Configure(opts, endpointConfig, transportConfig, routingConfig);

        transportConfig.SchemaAndCatalog.UseCatalogForQueue(opts.AuditQueue, "nservicebus");
    }
}