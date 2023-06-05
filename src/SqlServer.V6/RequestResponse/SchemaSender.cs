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

        transportConfig.DefaultSchema(MultiSchema.Sender);
        transportConfig.UseSchemaForQueue(opts.AuditQueue, MultiSchema.Audit);
        transportConfig.UseSchemaForEndpoint(opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)), MultiSchema.Receiver);

        var routing = transportConfig.Routing();
        routing.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)));
    }
}