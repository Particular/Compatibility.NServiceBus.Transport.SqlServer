using NServiceBus;
using NServiceBus.Compatibility;

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