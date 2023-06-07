using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Compatibility;

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

    protected override async Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
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

class Receiver : Base
{
    public class MyRequestHandler : IHandleMessages<MyRequest>
    {
        public Task Handle(MyRequest message, IMessageHandlerContext context)
        {
            return context.Reply(new MyResponse());
        }
    }
}