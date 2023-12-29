namespace NServiceBus.Compatibility.TestRunner.SqlServer.Tests;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NuGet.Versioning;
using NUnit.Framework;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class RequestResponse
{
    [OneTimeSetUp]
    public async Task SetUp()
    {
        await SqlHelper.CreateSchema(Global.ConnectionString, "receiver").ConfigureAwait(false);
        await SqlHelper.CreateSchema(Global.ConnectionString, "sender").ConfigureAwait(false);
    }

    [Test]
    [TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[4,)")]
    public async Task SingleSchema(NuGetVersion senderVersion, NuGetVersion receiverVersion)
    {
        if (senderVersion.Major < 6 || receiverVersion.Major < 6)
        {
            Assert.Ignore("Versions before 6 are incompatible with .NET 8 and System.Data.SqlClient");
        }

        var connectionStrings = new Dictionary<string, string>()
        {
            ["Sender"] = Global.ConnectionString + $";App=Sender",
            ["Receiver"] = Global.ConnectionString + $";App=Receiver",
        };

        var result = await SqlTransportScenarioRunner.Run("Sender", "Receiver", senderVersion, receiverVersion, x => x.Count == 2, connectionStrings).ConfigureAwait(false);

        Assert.True(result.Succeeded);

        Assert.AreEqual(2, result.AuditedMessages.Count, "Number of messages in audit queue");

        var request = result.AuditedMessages.Single(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Send));
        var response = result.AuditedMessages.Single(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Reply));

        Assert.AreEqual(request.Headers[Headers.MessageId], response.Headers[Headers.RelatedTo]);
        Assert.AreEqual(request.Headers[Headers.ConversationId], response.Headers[Headers.ConversationId]);
        Assert.AreEqual(request.Headers[Headers.CorrelationId], response.Headers[Headers.CorrelationId]);

        var requestVersion = SemanticVersion.Parse(request.Headers[Keys.WireCompatVersion]);
        var responseVersion = SemanticVersion.Parse(response.Headers[Keys.WireCompatVersion]);
        Assert.AreEqual(senderVersion, requestVersion);
        Assert.AreEqual(receiverVersion, responseVersion);
    }

    [Test]
    [TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[6,)")]
    public async Task MultiSchema(NuGetVersion senderVersion, NuGetVersion receiverVersion)
    {
        var connectionStrings = new Dictionary<string, string>()
        {
            ["SchemaSender"] = Global.ConnectionString + $";App=SchemaSender",
            ["SchemaReceiver"] = Global.ConnectionString + $";App=SchemaReceiver",
        };

        var result = await SqlTransportScenarioRunner.Run("SchemaSender", "SchemaReceiver", senderVersion, receiverVersion, x => x.Count == 2, connectionStrings).ConfigureAwait(false);

        Assert.True(result.Succeeded);

        var request = result.AuditedMessages.Single(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Send));
        var response = result.AuditedMessages.Single(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Reply));

        Assert.AreEqual(request.Headers[Headers.MessageId], response.Headers[Headers.RelatedTo]);
        Assert.AreEqual(request.Headers[Headers.ConversationId], response.Headers[Headers.ConversationId]);
        Assert.AreEqual(request.Headers[Headers.CorrelationId], response.Headers[Headers.CorrelationId]);

        var requestVersion = SemanticVersion.Parse(request.Headers[Keys.WireCompatVersion]);
        var responseVersion = SemanticVersion.Parse(response.Headers[Keys.WireCompatVersion]);
        Assert.AreEqual(senderVersion, requestVersion);
        Assert.AreEqual(receiverVersion, responseVersion);
    }

    [Test]
    [TestCaseSourcePackageSupportedVersions("NServiceBus.SqlServer", "[6,)")]
    public async Task MultiCatalog(NuGetVersion senderVersion, NuGetVersion receiverVersion)
    {
        var connectionStrings = new Dictionary<string, string>()
        {
            ["CatalogSender"] = Global.ConnectionString.Replace(MultiCatalogMap.Audit, MultiCatalogMap.Sender) + $";App=CatalogSender",
            ["CatalogReceiver"] = Global.ConnectionString.Replace(MultiCatalogMap.Audit, MultiCatalogMap.Receiver) + $";App=CatalogReceiver",
        };

        var result = await SqlTransportScenarioRunner.Run("CatalogSender", "CatalogReceiver", senderVersion, receiverVersion, x => x.Count == 2, connectionStrings).ConfigureAwait(false);

        Assert.True(result.Succeeded);

        var request = result.AuditedMessages.Single(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Send));
        var response = result.AuditedMessages.Single(x => x.Headers[Headers.MessageIntent] == nameof(MessageIntent.Reply));

        Assert.AreEqual(request.Headers[Headers.MessageId], response.Headers[Headers.RelatedTo]);
        Assert.AreEqual(request.Headers[Headers.ConversationId], response.Headers[Headers.ConversationId]);
        Assert.AreEqual(request.Headers[Headers.CorrelationId], response.Headers[Headers.CorrelationId]);

        var requestVersion = SemanticVersion.Parse(request.Headers[Keys.WireCompatVersion]);
        var responseVersion = SemanticVersion.Parse(response.Headers[Keys.WireCompatVersion]);
        Assert.AreEqual(senderVersion, requestVersion);
        Assert.AreEqual(receiverVersion, responseVersion);
    }
}