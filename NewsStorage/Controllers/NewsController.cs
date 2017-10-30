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
        private ISubscriptionsService subscriptionsService;
        private ILogger<NewsController> logger;

        public NewsController(ApplicationDbContext db, ISubscriptionsService subscriptionsService,
            ILogger<NewsController> logger)
        {
            this.db = db;
            this.logger = logger;
            this.subscriptionsService = subscriptionsService;
        }

        [HttpGet("{name}")]
        public async Task<List<string>> GetNewsForUser([FromRoute]string name, int page, int perpage)
        {
            logger.LogDebug($"Retrieving news for user {name}");
            var authors = await subscriptionsService.GetSubscribedAuthorsForName(name, 0, 0);
            if (authors != null && authors.Count > 0)
            {
                logger.LogDebug($"Authors for user {name}: {string.Join(", ", authors)} ({authors?.Count})");
                var news = db.News.Where(n => authors.Contains(n.Author));
                logger.LogDebug($"Found {news.Count()} news for user {name}");
                news = news.OrderByDescending(n => n.Date);
                if (perpage != 0 && page != 0)
                {
                    logger.LogDebug($"Skipping {perpage * page} news due to pagination");
                    news = news.Skip(perpage * page);
                }
                if (perpage != 0)
                {
                    logger.LogDebug($"Retrieving at max {perpage} news");
                    news = news.Take(perpage);
                }
                logger.LogDebug($"Returning {news.Count()} news");
                return news.Select(n => $"Header: {n.Header}{Environment.NewLine}Body: {n.Body}{Environment.NewLine}Author: {n.Author}")
                    .ToList();
            }
            logger.LogWarning($"No subscribed authors found for user {name}");
            return new List<string>();
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNews([FromBody] NewsModel newsModel)
        {
            var prevNews = db.News.FirstOrDefault(n => n.Author == newsModel.Author && n.Header == newsModel.Header);
            if (prevNews == null)
            {
                logger.LogDebug($"Adding news, author: {newsModel.Author}");
                if (newsModel.Date == null)
                    newsModel.Date = DateTime.Now;
                var state = db.News.Add(new News(newsModel))?.State;
                logger.LogDebug($"News addition result: {state}");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogWarning($"News already exists");
                return BadRequest();
            }
        }

        [HttpGet("author/{name}")]
        public async Task<List<string>> GetNewsByUser(string name, int page, int perpage)
        {
            logger.LogDebug($"Retrieving news by user {name}");
            var news = db.News.Where(n => n.Author == name);
            logger.LogDebug($"Found {news.Count()} news by user {name}");
            news = news.OrderByDescending(n => n.Date);
            if (perpage != 0 && page != 0)
            {
                logger.LogDebug($"Skipping {perpage * page} news due to pagination");
                news = news.Skip(perpage * page);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Retrieving at max {perpage} news");
                news = news.Take(perpage);
            }
            logger.LogDebug($"Returning {news.Count()} news");
            return news.Select(n => $"Header: {n.Header}{Environment.NewLine}Body: {n.Body}{Environment.NewLine}Author: {n.Author}")
                .ToList();
        }

        [HttpDelete("author/{name}")]
        public async Task<IActionResult> DeleteNewsByUser(string name)
        {
            var news = db.News.Where(n => n.Author == name);
            logger.LogDebug($"News by {name} count: {news.Count()}");
            if (news.Count() > 0)
            {
                db.News.RemoveRange(news);
                db.SaveChanges();
            }
            return Ok(news.Select(n => new NewsModel { Author = n.Author, Body = n.Body, Date = n.Date, Header = n.Header }).ToList());
        }

    }
}
