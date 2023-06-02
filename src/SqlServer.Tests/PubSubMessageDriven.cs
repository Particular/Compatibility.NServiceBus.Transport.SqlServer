namespace NServiceBus.Compatibility.TestRunner.SqlServer.Tests;

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
    public async Task Simple(NuGetVersion publisherVersion, NuGetVersion subscriberVersion)
    {
        var result = await SqlTransportScenarioRunner.Run(
            "MessageDrivenPublisher",
            "MessageDrivenSubscriber",
            publisherVersion,
            subscriberVersion,
            x => x.Count == 1
            )
            .ConfigureAwait(false);

        Assert.True(result.Succeeded);
        Assert.AreEqual(1, result.AuditedMessages.Count, "Audit queue message count");
        Assert.True(result.AuditedMessages.All(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Publish)), "No event message in audit queue");

        var eventVersion = SemanticVersion.Parse(result.AuditedMessages.First().Headers[Keys.WireCompatVersion]);
        Assert.AreEqual(publisherVersion, eventVersion);
    }
}