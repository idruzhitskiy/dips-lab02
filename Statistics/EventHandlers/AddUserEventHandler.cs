using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Statistics.EventBus;
using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.EventHandlers
{
    public class AddUserEventHandler : EventHandler<AddUserEvent>
    {
        private ILogger<AddUserEventHandler> logger;

        public AddUserEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<AddUserEventHandler> logger)
            : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(AddUserEvent @event)
        {
            logger.LogWarning($"Processing {@event.GetType().Name} {@event}");
        }
    }
}
