using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SubscriptionManager.Controllers
{
    [Route("api")]
    public class SubscriptionsController : Controller
    {
        private ApplicationDbContext db;
        private ILogger<SubscriptionsController> logger;

        public SubscriptionsController(ApplicationDbContext db, ILogger<SubscriptionsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpGet("{subscriber}")]
        public async Task<List<string>> GetAuthorsForName(string subscriber)
        {
            logger.LogDebug($"Retriving subscriptions for user {subscriber}");
            List<string> result = db.Subscriptions.Where(s => s.Subscriber.ToLowerInvariant() == subscriber.ToLowerInvariant()).Select(s => s.Author).ToList();
            logger.LogDebug($"Retrieved {result.Count} subscribers: {string.Join(", ", result)}");
            return result;
        }

        [HttpPost("{subscriber}")]
        public async Task<IActionResult> AddSubscription(string subscriber, string author)
        {
            logger.LogDebug($"Adding subscription: {subscriber} subscribes on feed of {author}");
            var subscription = new Subscription { Subscriber = subscriber.ToLowerInvariant(), Author = author.ToLowerInvariant() };
            var state = (await db.Subscriptions.AddAsync(subscription)).State;
            if (state == EntityState.Added)
            {
                logger.LogDebug($"Subscription {subscriber}-{author} added");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Subscription failed to add, result: {state}");
            return BadRequest();
        }

        [HttpDelete("{subscriber}/{author}")]
        public async Task<IActionResult> RemoveSubscription(string subscriber, string author)
        {
            logger.LogDebug($"Removing subscription {subscriber}-{author}");
            var subscription = await db.Subscriptions.FirstOrDefaultAsync(s => s.Subscriber.ToLowerInvariant() == subscriber.ToLowerInvariant() && s.Author.ToLowerInvariant() == author.ToLowerInvariant());
            if (subscription != null)
            {
                logger.LogDebug($"Subscription {subscriber}-{author} exists in database");
                var state = db.Subscriptions.Remove(subscription).State;
                if (state == EntityState.Deleted)
                {
                    logger.LogDebug($"Subscription {subscriber}-{author} removed");
                    db.SaveChanges();
                    return Ok();
                }
                logger.LogWarning($"Subscription {subscriber}-{author} failed to remove, result: {state}");
            }
            else
                logger.LogWarning($"Subscription {subscriber}-{author} not found");
            return BadRequest();
        }
    }
}
