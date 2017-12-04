using Microsoft.Extensions.Logging;
using Statistics.EventBus;
using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.EventHandlers
{
    public class AddNewsEventHandler : EventHandler<AddNewsEvent>
    {
        private ILogger<AddNewsEventHandler> logger;

        public AddNewsEventHandler(IEventBus eventBus, ILogger<AddNewsEventHandler> logger)
             : base(eventBus)
        {
            this.logger = logger;
        }

        public async override Task Handle(AddNewsEvent @event)
        {
            logger.LogWarning($"Processing {@event.GetType().Name} {@event}");
        }
    }
}
