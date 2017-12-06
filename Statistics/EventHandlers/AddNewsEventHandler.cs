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
    public class AddNewsEventHandler : EventHandler<AddNewsEvent>
    {
        private ILogger<AddNewsEventHandler> logger;

        public AddNewsEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<AddNewsEventHandler> logger)
             : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(AddNewsEvent @event)
        {
            try
            {
                logger.LogInformation($"Processing {@event.GetType().Name} {@event}");
                Entities.NewsAdditionInfo entity = new Entities.NewsAdditionInfo
                {
                    Id = @event.Id + @event.GetType().Name,
                    AddedTime = @event.OccurenceTime,
                    Author = @event.Author
                };
                if (dbContext.NewsAdditions.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.NewsAdditions.Add(entity);
                    dbContext.SaveChanges();
                }
                eventBus.Publish(new AckEvent { AdjEventId = @event.Id, Status = AckStatus.Success });
            }
            catch (Exception e)
            {
                eventBus.Publish(new AckEvent { AdjEventId = @event.Id, Description = e.ToString(), Status = AckStatus.Failed });
                logger.LogCritical(e.ToString());
            }
        }
    }
}
