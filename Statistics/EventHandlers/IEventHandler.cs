using Microsoft.Extensions.Logging;
using Statistics.EventBus;
using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.EventHandlers
{
    public interface IEventHandler { }

    public abstract class EventHandler<T> : IEventHandler
        where T : Event
    {
        public EventHandler(IEventBus eventBus)
        {
            eventBus.Subscribe(this);
        }
        public abstract Task Handle(T @event);
    }

}
