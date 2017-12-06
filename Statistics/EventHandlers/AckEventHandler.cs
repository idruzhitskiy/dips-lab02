﻿using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Microsoft.Extensions.Logging;
using Statistics.Misc;

namespace Statistics.EventHandlers
{
    public class AckEventHandler : EventHandler<AckEvent>
    {
        private ILogger<AckEventHandler> logger;
        private IEventStorage eventStorage;

        public AckEventHandler(IEventBus eventBus, 
            ILogger<AckEventHandler> logger,
            IEventStorage eventStorage) : base(eventBus, null)
        {
            this.logger = logger;
            this.eventStorage = eventStorage;
        }

        public override async Task Handle(AckEvent @event)
        {
            switch(@event.Status)
            {
                case AckStatus.Success:
                    eventStorage.RemoveEvent(@event.AdjEventId);
                    logger.LogInformation($"Event {@event.AdjEventId} successfull");
                    break;
                case AckStatus.Failed:
                    eventStorage.RemoveEvent(@event.AdjEventId);
                    logger.LogError($"Error acknowledging event {@event.AdjEventId}, exception: {@event.Description}");
                    break;
            }
        }
    }
}
