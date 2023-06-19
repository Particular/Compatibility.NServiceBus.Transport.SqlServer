using NServiceBus;
using NServiceBus.Compatibility;

class SchemaSender : Sender
{
    protected override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
        transportConfig.DefaultSchema = MultiSchemaMap.Sender;
        transportConfig.SchemaAndCatalog.UseSchemaForQueue(opts.AuditQueue, MultiSchemaMap.Audit);
        transportConfig.SchemaAndCatalog.UseSchemaForQueue(opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)), MultiSchemaMap.Receiver);

        routingConfig.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)));
    }
}

class SchemaReceiver : Receiver
{
    protected override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
        base.Configure(opts, endpointConfig, transportConfig, routingConfig);

        transportConfig.DefaultSchema = MultiSchemaMap.Receiver;
        transportConfig.SchemaAndCatalog.UseSchemaForQueue(opts.AuditQueue, MultiSchemaMap.Audit);
    }
}