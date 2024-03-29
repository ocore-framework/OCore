﻿using Orleans;
using Orleans.Streams;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OCore.Events
{
    public class Handler<T> : Grain,
        IAsyncObserver<Event<T>>,
        IEventHandler
    {       

        EventAttribute eventAttribute = null;
        EventAttribute EventAttribute
        {
            get
            {
                if (eventAttribute == null)
                {
                    eventAttribute = typeof(T).GetCustomAttribute<EventAttribute>();
                    if (eventAttribute == null)
                    {
                        throw new InvalidOperationException("Handler must have a [Handler]-attribute");
                    }
                }
                return eventAttribute;
            }
        }

        EventTypeOptions eventTypeOptions = null;
        EventTypeOptions EventTypeOptions
        {
            get
            {
                if (eventTypeOptions == null)
                {
                    eventTypeOptions = EventAttribute.Options;
                }
                return eventTypeOptions;
            }
        }

        HandlerAttribute EventHandlerAttribute
        {
            get
            {
                return GetType().GetCustomAttribute<HandlerAttribute>();
            }
        }

        string GetProviderName()
        {
            return EventAttribute?.Options?.ProviderName ?? "BaseStreamProvider";            
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider(GetProviderName());
            var stream = streamProvider.GetStream<Event<T>>($"{this.GetPrimaryKey()}/{FormatStreamNamespace(EventHandlerAttribute)}");
            await stream.SubscribeAsync(this);
        }

        private string FormatStreamNamespace(HandlerAttribute eventHandlerAttribute)
        {
            if (eventHandlerAttribute.Suffix == null)
            {
                return eventHandlerAttribute.EventName;
            }
            else
            {
                return $"{eventHandlerAttribute.EventName}:{eventHandlerAttribute.Suffix}";
            }
        }

        protected virtual Task HandleEvent(T @event)
        {
            return Task.CompletedTask;
        }

        protected virtual Task HandleEvent(Event<T> @event)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(Event<T> item, StreamSequenceToken token = null)
        {
            try
            {
                await CallHandlers(item);
            }
            catch
            {                
                bool poisonLimitReached = false;

                if (EventTypeOptions.TrackAndKillPoisonEvents == true)
                {
                    var failureTracker = GrainFactory.GetGrain<IPoisonEventCounter>(item.MessageId);
                    var failures = await failureTracker.Handle();
                    item.Retries = failures - 1;
                    if (failures >= EventTypeOptions.PoisonLimit)
                    {
                        var eventAggregator = GrainFactory.GetGrain<IEventAggregator>(0);
                        await eventAggregator.Raise(new PoisonEvent<T>(item), "poison");
                        poisonLimitReached = true;
                    }
                }

                bool @throw = poisonLimitReached == true
                              || EventHandlerAttribute.ContainExceptions == false;

                if (@throw == true)
                {
                    throw;
                }
            }
        }

        private async Task CallHandlers(Event<T> item)
        {
            await HandleEvent(item);
            await HandleEvent(item.Payload);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}
