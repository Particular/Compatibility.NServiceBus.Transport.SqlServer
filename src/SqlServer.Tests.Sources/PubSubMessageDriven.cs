namespace NServiceBus.Compatibility.TestRunner.SqlServer.Tests;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using NUnit.Framework;

[Parallelizable(ParallelScope.All)]
[TestFixture]
public class PubSubMessageDriven
{
    [Test]
    [TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[4,6)")]
    [Ignore("Versions before 6 are incompatible with .NET 8 and System.Data.SqlClient")]
    public async Task Simple(NuGetVersion publisherVersion, NuGetVersion subscriberVersion)
    {
        var connectionStrings = new Dictionary<string, string>()
        {
            ["MessageDrivenPublisher"] = Global.ConnectionString + $";App=MessageDrivenPublisher",
            ["MessageDrivenSubscriber"] = Global.ConnectionString + $";App=MessageDrivenSubscriber",
        };

        var result = await SqlTransportScenarioRunner.Run(
            "MessageDrivenPublisher",
            "MessageDrivenSubscriber",
            publisherVersion,
            subscriberVersion,
            x => x.Count == 1,
            connectionStrings
            )
            .ConfigureAwait(false);

        Assert.True(result.Succeeded);
        Assert.AreEqual(1, result.AuditedMessages.Count, "Audit queue message count");
        Assert.True(result.AuditedMessages.All(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Publish)), "No event message in audit queue");

        var eventVersion = SemanticVersion.Parse(result.AuditedMessages.First().Headers[Keys.WireCompatVersion]);
        Assert.AreEqual(publisherVersion, eventVersion);
    }
}