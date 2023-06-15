using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;

class DiscardBehavior : IBehavior<IIncomingPhysicalMessageContext, IIncomingPhysicalMessageContext>
{
    readonly string TestRunId;

    public DiscardBehavior(string testRunId)
    {
        TestRunId = testRunId;
    }

    public Task Invoke(IIncomingPhysicalMessageContext context, Func<IIncomingPhysicalMessageContext, Task> next)
    {
        if (context.MessageHeaders.TryGetValue(Headers.MessageIntent, out var intent) && intent == nameof(MessageIntentEnum.Subscribe))
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