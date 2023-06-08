namespace NServiceBus.Compatibility.TestRunner.SqlServer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

static class SqlTransportScenarioRunner
{
    public static long RunCounter;
    static readonly ObjectPool<long> Pool = new(() => Interlocked.Increment(ref RunCounter));
    static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(Debugger.IsAttached ? 600 : 30);

    public static async Task<TestExecutionResult> Run(
        string aTypeNameBehavior,
        string bTypeNameBehavior,
        SemanticVersion a,
        SemanticVersion b,
        Func<List<AuditMessage>, bool> doneCallback,
        Dictionary<string, string> connectionStrings
        )
    {
        using var cts = new CancellationTokenSource(TestTimeout);
        var cancellationToken = cts.Token;
        var connectionString = Global.ConnectionString;

        var runCount = Pool.Get();
        try
        {
            var testRunId = Guid.NewGuid().ToString();

            var opts = new PluginOptions
            {
                ConnectionStrings = connectionStrings,
                TestRunId = testRunId,
                RunCount = runCount,
            };

            await SqlHelper.DropTablesWithPrefix(Global.ConnectionString, opts.ApplyUniqueRunPrefix(string.Empty), cancellationToken).ConfigureAwait(false);

            opts.AuditQueue = opts.ApplyUniqueRunPrefix("AuditSpy");

            var auditSpyTransport = new SqlServerTransport(connectionString + ";App=AuditSpy")
            {
                TransportTransactionMode = TransportTransactionMode.ReceiveOnly,
            };
            auditSpyTransport.Subscriptions.SubscriptionTableName = new Transport.SqlServer.SubscriptionTableName(opts.ApplyUniqueRunPrefix("SubscriptionRouting"));

            var agents = new[]
            {
                AgentInfo.Create(aTypeNameBehavior, a, opts),
                AgentInfo.Create(bTypeNameBehavior, b, opts),
            };

            var result = await TestScenarioPluginRunner
                .Run(opts, agents, auditSpyTransport, doneCallback, cancellationToken)
                .ConfigureAwait(false);

            result.AuditedMessages = result.AuditedMessages;
            return result;
        }
        finally
        {
            Pool.Return(runCount);
        }
    }
}