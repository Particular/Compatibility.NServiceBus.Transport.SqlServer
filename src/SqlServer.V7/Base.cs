using System.Threading.Tasks;
using System.Threading;
using NServiceBus;
using NServiceBus.Compatibility;

abstract class Base : ITestBehavior
{
    public EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;

        var config = new EndpointConfiguration(opts.ApplyUniqueRunPrefix(endpointName));

        var connectionString = opts.ConnectionString + $";App={endpointName}";

        if (endpointName.StartsWith("Catalog"))
        {
            if (endpointName.EndsWith("Receiver"))
            {
                connectionString = connectionString.Replace("nservicebus", "nservicebus1");
            }
            else
            {
                connectionString = connectionString.Replace("nservicebus", "nservicebus2");
            }
        }

        var transport = new SqlServerTransport(connectionString)
        {
            //TransportTransactionMode = TransportTransactionMode.ReceiveOnly,
            TransportTransactionMode = TransportTransactionMode.SendsAtomicWithReceive,
        };

        transport.Subscriptions.SubscriptionTableName = new NServiceBus.Transport.SqlServer.SubscriptionTableName(opts.ApplyUniqueRunPrefix("SubscriptionRouting"));

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