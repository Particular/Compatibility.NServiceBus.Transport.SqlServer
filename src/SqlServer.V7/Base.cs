using System.Threading.Tasks;
using System.Threading;
using NServiceBus;
using NServiceBus.Compatibility;

abstract class Base : ITestBehavior
{
    public EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;

        var config = new EndpointConfiguration(endpointName);

        var transport = new SqlServerTransport(opts.ConnectionString + $";App={endpointName}")
        {
            TransportTransactionMode = TransportTransactionMode.ReceiveOnly,
        };

        var routingConfig = config.UseTransport(transport);

        Configure(opts, config, transport, routingConfig);

        return config;
    }

    protected virtual void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        SqlServerTransport transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
        )
    {
    }

    public virtual Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}