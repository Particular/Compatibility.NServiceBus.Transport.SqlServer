namespace NServiceBus.Compatibility.TestRunner.SqlServer.Tests;

using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NuGet.Versioning;
using NUnit.Framework;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class PubSubNative
{
    [Test]
    [TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[5,)")]
    public async Task Simple(NuGetVersion subscriberVersion, NuGetVersion publisherVersion)
    {
        var result = await SqlTransportScenarioRunner.Run(
            "Subscriber",
            "Publisher",
            subscriberVersion,
            publisherVersion,
            x => x.Count == 2
            )
            .ConfigureAwait(false);

        Assert.True(result.Succeeded);
        Assert.AreEqual(2, result.AuditedMessages.Count, "Audit queue message count");
        Assert.True(result.AuditedMessages.All(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Publish)), "No event message in audit queue");

        var eventVersion = SemanticVersion.Parse(result.AuditedMessages.First().Headers[Keys.WireCompatVersion]);
        Assert.AreEqual(publisherVersion, eventVersion);
    }
}