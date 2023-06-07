using NServiceBus;
using NServiceBus.Compatibility;

class SchemaReceiver : Receiver
{
    public override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
    )
    {
        base.Configure(opts, endpointConfig, transportConfig);

        transportConfig.DefaultSchema(MultiSchemaMap.Receiver);
        transportConfig.UseSchemaForQueue(opts.AuditQueue, MultiSchemaMap.Audit);
    }
}

class SchemaSender : Sender
{
    public override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
        )
    {

        transportConfig.DefaultSchema(MultiSchemaMap.Sender);
        transportConfig.UseSchemaForQueue(opts.AuditQueue, MultiSchemaMap.Audit);
        transportConfig.UseSchemaForEndpoint(opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)), MultiSchemaMap.Receiver);

        var routing = transportConfig.Routing();
        routing.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)));
    }
}