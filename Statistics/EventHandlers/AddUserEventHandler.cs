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
            try
            {
                var eventDescription = $"{@event.GetType().Name} { @event}";
                logger.LogInformation($"Processing {eventDescription}");
                Entities.UserOperationInfo entity = new Entities.UserOperationInfo
                {
                    Operation = Entities.Operation.Register,
                    Subject = @event.Username,
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
