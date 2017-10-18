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
        public async Task<List<string>> GetAuthorsForName(string subscriber, int page, int perpage)
        {
            logger.LogDebug($"Retriving subscriptions for user {subscriber}");
            var result = db.Subscriptions.Where(s => s.Subscriber.ToLowerInvariant() == subscriber.ToLowerInvariant());
            logger.LogDebug($"Found {result.Count()} authors for user {subscriber}");
            if (page != 0 && perpage != 0)
            {
                logger.LogDebug($"Skipping {page * perpage} entities due to pagination");
                result = result.Skip(page * perpage);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Taking at max {perpage} entities");
                result = result.Take(perpage);
            }
            logger.LogDebug($"Retrieved {result.Count()} authors: {string.Join(", ", result)}");
            return result.Select(s => s.Author).ToList();
        }

        [HttpPost("{subscriber}")]
        public async Task<IActionResult> AddSubscription(string subscriber, string author)
        {
            var prevSubscription = db.Subscriptions.FirstOrDefault(s => s.Subscriber == subscriber.ToLowerInvariant() && s.Author == author.ToLowerInvariant());
            if (prevSubscription == null)
            {
                logger.LogDebug($"Adding subscription: {subscriber} subscribes on feed of {author}");
                var subscription = new Subscription { Subscriber = subscriber.ToLowerInvariant(), Author = author.ToLowerInvariant() };
                var state = db.Subscriptions.Add(subscription)?.State;
                logger.LogDebug($"Subscription {subscriber}-{author} added with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Subscription {subscriber}-{author} already exists");
            return BadRequest();
        }

        [HttpDelete("{subscriber}/{author}")]
        public async Task<IActionResult> RemoveSubscription(string subscriber, string author)
        {
            logger.LogDebug($"Removing subscription {subscriber}-{author}");
            var subscription = db.Subscriptions.FirstOrDefault(s => s.Subscriber == subscriber.ToLowerInvariant() && s.Author == author.ToLowerInvariant());
            if (subscription != null)
            {
                logger.LogDebug($"Subscription {subscriber}-{author} exists in database");
                var state = db.Subscriptions.Remove(subscription)?.State;
                logger.LogDebug($"Subscription {subscriber}-{author} removed with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Subscription {subscriber}-{author} not found");
            return BadRequest();
        }
    }
}
