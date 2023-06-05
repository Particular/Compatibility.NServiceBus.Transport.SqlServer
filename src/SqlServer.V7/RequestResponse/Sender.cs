using System.Threading;
using System.Threading.Tasks;
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
        transportConfig.DefaultSchema = MultiSchema.Sender;
        transportConfig.SchemaAndCatalog.UseSchemaForQueue(opts.AuditQueue, MultiSchema.Audit);
        transportConfig.SchemaAndCatalog.UseSchemaForQueue(opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)), MultiSchema.Receiver);

        routingConfig.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(SchemaReceiver)));
    }
}

class Sender : Base
{
    protected override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
        routingConfig.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(Receiver)));
    }

    public override async Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        await endpointInstance.Send(new MyRequest(), cancellationToken).ConfigureAwait(false);
    }

    public class MyResponseHandler : IHandleMessages<MyResponse>
    {
        public Task Handle(MyResponse message, IMessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }
}