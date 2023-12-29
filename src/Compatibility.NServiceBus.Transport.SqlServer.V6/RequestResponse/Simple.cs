using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Compatibility;

class Sender : Base
{
    public override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
        )
    {
        var routing = transportConfig.Routing();
        routing.RouteToEndpoint(typeof(MyRequest), opts.ApplyUniqueRunPrefix(nameof(Receiver)));
    }

    protected override async Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        try
        {
            await endpointInstance.Send(new MyRequest()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("FAIL!" + ex);
            Console.WriteLine("FAIL!" + ex);
            throw;
        }

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