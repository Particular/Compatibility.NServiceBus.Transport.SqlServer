namespace NServiceBus.Compatibility;

using System;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transport;

class StampVersionBehavior : Behavior<IOutgoingPhysicalMessageContext>
{
    string versionString;

    public StampVersionBehavior(IDispatchMessages dispatcher)
    {
        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(dispatcher.GetType().Assembly.Location);
        versionString = fileVersionInfo.ProductVersion;
    }

    public override Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
    {
        context.Headers[Keys.WireCompatVersion] = versionString;
        return next();
    }
}