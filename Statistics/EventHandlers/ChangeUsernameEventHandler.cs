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
    public class ChangeUsernameEventHandler : EventHandler<ChangeUsernameEvent>
    {
        private ILogger<ChangeUsernameEventHandler> logger;

        public ChangeUsernameEventHandler(IEventBus eventBus, DbProxy dbProxy, ILogger<ChangeUsernameEventHandler> logger) : base(eventBus, dbProxy)
        {
            this.logger = logger;
        }

        public override async Task Handle(ChangeUsernameEvent @event)
        {
            try
            {
                var eventDescription = $"{@event.GetType().Name} { @event}";
                logger.LogInformation($"Processing {eventDescription}");
                Entities.UserOperationInfo entity = new Entities.UserOperationInfo
                {
                    Operation = Entities.UserOperation.NameChanged,
                    Subject = @event.NewUsername,
                    Time = @event.OccurenceTime,
                    Id = @event.Id + @event.GetType().Name
                };
                if (dbContext.UserOperations.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.UserOperations.Add(entity);
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
