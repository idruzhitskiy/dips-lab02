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
    public class LoginEventHandler : EventHandler<LoginEvent>
    {
        private ILogger<LoginEventHandler> logger;

        public LoginEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<LoginEventHandler> logger)
            : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(LoginEvent @event)
        {
            logger.LogWarning($"Processing {@event.GetType().Name} {@event}");
        }
    }
}
