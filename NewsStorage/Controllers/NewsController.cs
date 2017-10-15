using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsStorage.Entities;
using Microsoft.Extensions.Configuration;
using Gateway.Services;
using Gateway.Models;
using Microsoft.Extensions.Logging;

namespace NewsStorage.Controllers
{
    [Route("api")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db;
        private SubscriptionsService subscriptions;
        private ILogger<NewsController> logger;

        public NewsController(ApplicationDbContext db, IConfiguration configuration,
            ILogger<NewsController> logger)
        {
            this.db = db;
            subscriptions = new SubscriptionsService(configuration.GetSection("Addresses")["Subscriptions"]);
            this.logger = logger;
        }

        [HttpGet("{name}")]
        public async Task<List<string>> GetNewsForUser([FromRoute]string name)
        {
            logger.LogDebug($"Retrieving news for user {name}");
            var authors = await subscriptions.GetSubscribedAuthorsForName(name);
            logger.LogDebug($"Authors for user {name}: {string.Join(", ", authors)} ({authors.Count})");
            var news = db.News.Where(n => authors.Contains(n.Author));
            logger.LogDebug($"Found {news.Count()} news for user {name}");
            return news
                .OrderByDescending(n => n.Date)
                .Select(n => $"{n.Header}\n{n.Body}\n{n.Author}")
                .ToList();
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNews([FromBody] NewsModel newsModel)
        {
            logger.LogDebug($"Adding news, author: {newsModel.Author}");
            var news = new News(newsModel);
            news.Date = DateTime.Now;
            var state = (await db.News.AddAsync(news)).State;
            if (state == EntityState.Added)
            {
                logger.LogDebug($"News added");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogWarning($"Adding news failed, result: {state}");
                return BadRequest();
            }
        }
    }
}
