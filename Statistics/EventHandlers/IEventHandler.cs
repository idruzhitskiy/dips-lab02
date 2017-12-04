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

    public abstract class EventHandler<T> : IEventHandler, IDisposable
        where T : Event
    {
        protected ApplicationDbContext dbContext;
        private IEventBus eventBus;

        public EventHandler(IEventBus eventBus, DbProxy dbProxy)
        {
            this.eventBus = eventBus;
            eventBus.Subscribe(this);
            this.dbContext = dbProxy.DbContext;
        }

        public void Dispose()
        {
            eventBus.Unsubscribe(this);
        }

        public abstract Task Handle(T @event);
    }

}
