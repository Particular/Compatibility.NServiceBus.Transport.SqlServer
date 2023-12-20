namespace NServiceBus.Compatibility;

using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;

class DiscardBehavior(string testRunId) : IBehavior<IIncomingPhysicalMessageContext, IIncomingPhysicalMessageContext>
{
    readonly string TestRunId = testRunId;

    public Task Invoke(IIncomingPhysicalMessageContext context, Func<IIncomingPhysicalMessageContext, Task> next)
    {
        if (context.MessageHeaders.TryGetValue(Headers.MessageIntent, out var intent) && intent == nameof(MessageIntent.Subscribe))
        {
            //Subscribe messages don't get stamped with test run it
            return next(context);
        }

        if (!context.MessageHeaders.TryGetValue("TestRunId", out var testRunId) || testRunId != TestRunId)
        {
            return Task.CompletedTask;
        }

        return next(context);
    }
}