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
            return db.Subscriptions.Where(s => s.Subscriber == subscriber).Select(s => s.Author).ToList();
        }

        [HttpPost("{subscriber}")]
        public async Task<IActionResult> AddSubscription(string subscriber, string author)
        {
            var subscription = new Subscription { Subscriber = subscriber, Author = author };
            if ((await db.Subscriptions.AddAsync(subscription)).State == EntityState.Added)
                return Ok();
            return BadRequest();
        }

        [HttpDelete("{subscriber}/{author}")]
        public async Task<IActionResult> RemoveSubscription(string subscriber, string author)
        {
            var subscription = await db.Subscriptions.FirstOrDefaultAsync(s => s.Subscriber == subscriber && s.Author == author);
            if (subscription != null)
                if (db.Subscriptions.Remove(subscription).State == EntityState.Deleted)
                    return Ok();
            return BadRequest();
        }
    }
}
