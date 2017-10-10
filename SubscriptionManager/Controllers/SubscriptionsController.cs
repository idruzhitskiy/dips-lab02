using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace SubscriptionManager.Controllers
{
    [Route("api")]
    public class SubscriptionsController : Controller
    {
        private ApplicationDbContext db;

        public SubscriptionsController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet("{subscriber}")]
        public async Task<List<string>> GetAuthorsForName(string subscriber)
        {
            return db.Subscriptions.Where(s => s.Subscriber.ToLowerInvariant() == subscriber.ToLowerInvariant()).Select(s => s.Author).ToList();
        }

        [HttpPost("{subscriber}")]
        public async Task<IActionResult> AddSubscription(string subscriber, string author)
        {
            var subscription = new Subscription { Subscriber = subscriber.ToLowerInvariant(), Author = author.ToLowerInvariant() };
            if ((await db.Subscriptions.AddAsync(subscription)).State == EntityState.Added)
            {
                db.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{subscriber}/{author}")]
        public async Task<IActionResult> RemoveSubscription(string subscriber, string author)
        {
            var subscription = await db.Subscriptions.FirstOrDefaultAsync(s => s.Subscriber.ToLowerInvariant() == subscriber.ToLowerInvariant() && s.Author.ToLowerInvariant() == author.ToLowerInvariant());
            if (subscription != null)
                if (db.Subscriptions.Remove(subscription).State == EntityState.Deleted)
                {
                    db.SaveChanges();
                    return Ok();
                }
            return BadRequest();
        }
    }
}
