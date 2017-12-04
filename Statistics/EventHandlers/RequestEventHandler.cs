using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Statistics.EventHandlers
{
    public class RequestEventHandler : EventHandler<RequestEvent>
    {
        private ILogger<RequestEventHandler> logger;

        public RequestEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<RequestEventHandler> logger)
            : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(RequestEvent @event)
        {
            var eventDescription = $"{@event.GetType().Name} { @event}";
            logger.LogInformation($"Processing {eventDescription}");
            if (!@event.Host.EndsWith(".loc"))
            {
                logger.LogError($"Error processing {eventDescription}");
            }
            else
            {
                dbContext.Accesses.Add(new Entities.AccessInfo
                {
                    From = $"{@event.Origin}",
                    To = $"{@event.Host}{@event.Route}",
                    Time = @event.OccurenceTime
                });
                dbContext.SaveChanges();
            }
        }
    }
}
