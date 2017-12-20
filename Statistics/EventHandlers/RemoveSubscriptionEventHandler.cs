using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Microsoft.Extensions.Logging;

namespace Statistics.EventHandlers
{
    public class RemoveSubscriptionEventHandler : EventHandler<RemoveSubscriptionEvent>
    {
        private ILogger<RemoveSubscriptionEventHandler> logger;

        public RemoveSubscriptionEventHandler(IEventBus eventBus, DbProxy dbProxy, ILogger<RemoveSubscriptionEventHandler> logger) : base(eventBus, dbProxy)
        {
            this.logger = logger;
        }

        public override async Task Handle(RemoveSubscriptionEvent @event)
        {
            try
            {
                var eventDescription = $"{@event.GetType().Name} { @event}";
                logger.LogInformation($"Processing {eventDescription}");
                Entities.SubscriptionOperationInfo entity = new Entities.SubscriptionOperationInfo
                {
                    Operation = Entities.SubscriptionOperation.Added,
                    Author = @event.Author,
                    Subscriber = @event.Subscriber,
                    Time = @event.OccurenceTime,
                    Id = @event.Id + @event.GetType().Name
                };
                if (dbContext.UserOperations.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.SubscriptionOperations.Add(entity);
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
