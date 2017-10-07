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

namespace NewsStorage.Controllers
{
    [Route("api")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db;
        private SubscriptionsService subscriptions;

        public NewsController(ApplicationDbContext db, IConfiguration configuration)
        {
            this.db = db;
            subscriptions = new SubscriptionsService(configuration.GetSection("Addresses")["Subscriptions"]);
        }

        [HttpGet("{name}")]
        public async Task<List<string>> GetNewsForUser(string name)
        {
            var authors = await subscriptions.GetSubscribedAuthorsForName(name);
            var news = db.News.Where(n => authors.Contains(n.Author));
            return news
                .OrderByDescending(n => n.Date)
                .Select(n => $"{n.Header}\n{n.Body}\n{n.Author}")
                .ToList();
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNews([FromBody] NewsModel newsModel)
        {
            var news = new News(newsModel);
            news.Date = DateTime.Now;
            var result = await db.News.AddAsync(news);
            if (result.State == EntityState.Added)
                return Ok();
            else
                return BadRequest();
        }
    }
}
