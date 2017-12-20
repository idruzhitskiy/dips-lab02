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
    public class DeleteUserEventHandler : EventHandler<DeleteUserEvent>
    {
        private ILogger<DeleteUserEventHandler> logger;

        public DeleteUserEventHandler(IEventBus eventBus, DbProxy dbProxy, ILogger<DeleteUserEventHandler> logger) : base(eventBus, dbProxy)
        {
            this.logger = logger;
        }

        public override async Task Handle(DeleteUserEvent @event)
        {
            try
            {
                var eventDescription = $"{@event.GetType().Name} { @event}";
                logger.LogInformation($"Processing {eventDescription}");
                Entities.UserOperationInfo entity = new Entities.UserOperationInfo
                {
                    Operation = Entities.UserOperation.Delete,
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
