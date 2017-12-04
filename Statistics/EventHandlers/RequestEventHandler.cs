using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Microsoft.Extensions.Logging;

namespace Statistics.EventHandlers
{
    public class RequestEventHandler : EventHandler<RequestEvent>
    {
        private ILogger<RequestEventHandler> logger;

        public RequestEventHandler(IEventBus eventBus, ILogger<RequestEventHandler> logger) 
            : base(eventBus)
        {
            this.logger = logger;
        }

        public async override Task Handle(RequestEvent @event)
        {
            logger.LogWarning($"Processing {@event.GetType().Name} {@event}");
        }
    }
}
