﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.Pipeline;
using NServiceBus.Transport;
using NServiceBus.Compatibility;

class MessageDrivenPublisher : Base
{
    TaskCompletionSource<bool> subscribed = new();

    protected override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
        )
    {
        endpointConfig.Pipeline.Register(new SubscriptionBehavior(eventArgs => subscribed.SetResult(true), MessageIntentEnum.Subscribe), "Detects subscription");
    }

    protected override async Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        await subscribed.Task.ConfigureAwait(false);
        await endpointInstance.Publish(new MyEvent()).ConfigureAwait(false);
    }

    class SubscriptionBehavior : IBehavior<ITransportReceiveContext, ITransportReceiveContext>
    {
        public SubscriptionBehavior(Action<SubscriptionEventArgs> action, MessageIntentEnum intentToHandle)
        {
            this.action = action;
            this.intentToHandle = intentToHandle;
        }

        public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
        {
            await next(context).ConfigureAwait(false);
            var subscriptionMessageType = GetSubscriptionMessageTypeFrom(context.Message);
            if (subscriptionMessageType != null)
            {
                if (!context.Message.Headers.TryGetValue(Headers.SubscriberTransportAddress, out var returnAddress))
                {
                    context.Message.Headers.TryGetValue(Headers.ReplyToAddress, out returnAddress);
                }

                if (!context.Message.Headers.TryGetValue(Headers.SubscriberEndpoint, out var endpointName))
                {
                    endpointName = string.Empty;
                }

                var intent = (MessageIntentEnum)Enum.Parse(typeof(MessageIntentEnum), context.Message.Headers[Headers.MessageIntent], true);
                if (intent != intentToHandle)
                {
                    return;
                }

                action(new SubscriptionEventArgs
                {
                    MessageType = subscriptionMessageType,
                    SubscriberReturnAddress = returnAddress,
                    SubscriberEndpoint = endpointName
                });
            }
        }

        static string GetSubscriptionMessageTypeFrom(IncomingMessage msg)
        {
            return msg.Headers.TryGetValue(Headers.SubscriptionMessageType, out var headerValue) ? headerValue : null;
        }

        Action<SubscriptionEventArgs> action;
        MessageIntentEnum intentToHandle;
    }
}

class MessageDrivenSubscriber : Base
{
    protected override void Configure(
        PluginOptions args,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
        )
    {
        routingConfig.RegisterPublisher(typeof(MyEvent), args.ApplyUniqueRunPrefix(nameof(MessageDrivenPublisher)));
    }

    public class MyEventHandler : IHandleMessages<MyEvent>
    {
        public Task Handle(MyEvent message, IMessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }
}