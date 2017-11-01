using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Gateway.Models;
using Gateway.Services;
using Microsoft.Extensions.Logging;
using Gateway.Services.Implementations;
using Gateway.Scheduling;

namespace Gateway.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        private IAccountsService accountsService;
        private ISubscriptionsService subscriptionsService;
        private INewsService newsService;
        private ILogger<MainController> logger;

        public MainController(ILogger<MainController> logger,
            IAccountsService accountsService,
            ISubscriptionsService subscriptionsService,
            INewsService newsService)
        {
            this.logger = logger;
            this.accountsService = accountsService;
            this.subscriptionsService = subscriptionsService;
            this.newsService = newsService;
        }

        [HttpPost("user")]
        public async Task<IActionResult> Register(RegisterModel userModel)
        {
            var response = await accountsService.Register(userModel);
            logger.LogInformation($"Response from accounts service: {response?.StatusCode}");

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.LogInformation("Adding claim");
                logger.LogInformation($"Adding claim response: {response?.StatusCode}");
                return Ok();
            }
            else if (response != null)
            {
                string respContent = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                logger.LogError($"User {userModel.Username} not registered, error content: {respContent}");
                return BadRequest(respContent);
            }
            else
            {
                logger.LogCritical("Accounts service unavailable");
                return NotFound("Service unavailable");
            }
        }

        // Модифицирует несколько сервисов
        [HttpDelete("user/{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var accountsServerResult = await accountsService.DeleteUser(username);
            if (accountsServerResult == null)
            {
                return StatusCode(503, "Accounts service unavailable");
            }

            var newsServerResult = await newsService.DeleteNewsWithAuthor(username);
            if (newsServerResult == null)
            {
                var result = (await accountsService.Register(accountsServerResult))?.IsSuccessStatusCode == true;
                return StatusCode(503, $"News service unavailable, rollback status: Acc - {(result ? "ok" : "failed")}");
            }

            var subscriptionsServerResult = await subscriptionsService.RemoveAllAssociatedSubscriptions(username);
            if (subscriptionsServerResult == null)
            {
                var resultNews = true;
                foreach (var news in newsServerResult)
                    resultNews &= (await newsService.AddNews(news))?.IsSuccessStatusCode == true;
                var resultAccounts = (await accountsService.Register(accountsServerResult))?.IsSuccessStatusCode == true ;
                return StatusCode(503, $"News service unavailable, rollback status: Acc - { (resultAccounts ? "ok" : "failed")}, News - {(resultNews ? "ok" : "failed")}");
            }

            return Ok();
        }

        // Модифицирует несколько сервисов
        [HttpPost("user/{username}")]
        public async Task<IActionResult> ChangeUserName(string username, string newUsername)
        {
            var accountsResult = await accountsService.ChangeUserName(username, newUsername);
            var newsResult = await newsService.ChangeUserName(username, newUsername);
            var subscriptionsResult = await subscriptionsService.ChangeUserName(username, newUsername);

            if (accountsResult == null || newsResult == null || subscriptionsResult == null)
            {
                if (accountsResult != null)
                    await accountsService.ChangeUserName(newUsername, username);
                if (newsResult != null)
                    await newsService.ChangeUserName(newUsername, username);
                if (subscriptionsResult != null)
                    await subscriptionsService.ChangeUserName(newUsername, username);

                Scheduler.ScheduleRetryUntilSuccess(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync($"http://localhost/user/{username}", 
                            new FormUrlEncodedContent(new Dictionary<string, string> { { "newUsername", newUsername } }));
                        if (response.IsSuccessStatusCode)
                            return true;
                        return false;
                    }
                });
                return StatusCode(503, "Services status: " +
                    $"AS: {(accountsResult != null ? "online" : "offline")};" +
                    $"NS: {(newsResult != null ? "online" : "offline")};" +
                    $"SS: {(subscriptionsResult != null ? "online" : "offline")};");
            }

            var message = string.Empty;
            if (!accountsResult.IsSuccessStatusCode)
                message += $"AS operation status: {accountsResult.StatusCode}, {accountsResult.Content.ReadAsStringAsync().Result}";
            if (!newsResult.IsSuccessStatusCode)
                message += $"NS operation status: {newsResult.StatusCode}, {newsResult.Content.ReadAsStringAsync().Result}";
            if (!subscriptionsResult.IsSuccessStatusCode)
                message += $"SS operation status: {subscriptionsResult.StatusCode}, {subscriptionsResult.Content.ReadAsStringAsync().Result}";

            if (string.IsNullOrWhiteSpace(message))
                return Ok();
            return StatusCode(500, message);
        }

        // Запрашивает информацию с нескольких сервисов
        [HttpGet("{name}/news")]
        public async Task<IActionResult> GetNews(string name, int page = 0, int perpage = 0)
        {
            var message = string.Empty;
            var userExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = name });

            if (userExists == null)
                message = "Accounts service unavailable";
            else if (!userExists.IsSuccessStatusCode)
                message = $"User doesn't exist, response: {userExists}";
            else
            {
                List<string> authors = await subscriptionsService.GetSubscribedAuthorsForName(name, 0, 0);
                IEnumerable<string> news = Enumerable.Empty<string>();
                if (authors == null)
                {
                    logger.LogCritical("Subscriptions service unavailable");
                    news = await newsService.GetNews();
                }
                else
                {
                    foreach (var author in authors)
                        news = news.Concat((await newsService.GetNewsByUser(author, 0, 0)) ?? Enumerable.Empty<string>());
                }

                if (news != null)
                {
                    if (perpage != 0 && page != 0)
                        news = news.Skip(perpage * page);
                    if (perpage != 0)
                        news = news.Take(perpage);
                    return StatusCode(200, news);
                }
                else
                    message = "News service unavailable";
            }
            logger.LogCritical(message);
            return StatusCode(503, message);
        }

        [HttpPost("news")]
        public async Task<IActionResult> AddNews(NewsModel news)
        {
            var authorExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = news.Author });
            logger.LogInformation($"Author response: {authorExists?.StatusCode}");
            if (authorExists == null)
                return NotFound("Accounts service unavailable");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Author doesn't exists");

            var response = await newsService.AddNews(news);
            if (response != null)
            {
                logger.LogInformation($"Attempt to add news, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                    return Ok();
                else
                    return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("News service unavailable");
                return NotFound("Service unavailable");
            }
        }

        [HttpGet("{subscriber}/subscriptions")]
        public async Task<List<string>> GetSubscribedAuthors(string subscriber, int page = 0, int perpage = 0)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = subscriber });
            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}");

            if (subscriberExists == null || subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            logger.LogInformation($"Requesting authors for user {subscriber}");
            var authors = await subscriptionsService.GetSubscribedAuthorsForName(subscriber, page, perpage);
            if (authors != null)
            {
                logger.LogInformation($"Found {authors.Count()} authors for user {subscriber}");
                return authors;
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return null;
            }
        }

        [HttpPost("{subscriber}/subscriptions/{author}")]
        public async Task<IActionResult> AddSubscription(string subscriber, string author)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = subscriber });
            var authorExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = author });
            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}, author response: {authorExists?.StatusCode}");

            if (subscriberExists == null || authorExists == null)
                return NotFound("Accounts service unavailable");
            if (subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Subscriber doesn't exists");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Author doesn't exists");

            var response = await subscriptionsService.AddSubscription(subscriber, author);
            if (response != null)
            {
                logger.LogInformation($"Attempt to add subscription {subscriber}-{author}, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                    return Ok();
                else
                    return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return NotFound("Service unavailable");
            }
        }

        [HttpDelete("{subscriber}/subscriptions/{author}")]
        public async Task<IActionResult> RemoveSubscription(string subscriber, string author)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = subscriber });
            var authorExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = author });

            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}, author response: {authorExists?.StatusCode}");

            if (subscriberExists == null || authorExists == null)
                return NotFound("Accounts service unavailable");
            if (subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Subscriber doesn't exists");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Author doesn't exists");

            var response = await subscriptionsService.RemoveSubscription(subscriber, author);
            if (response != null)
            {
                logger.LogInformation($"Attempt to remove subscription {subscriber}-{author}, response {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                    return Ok();
                else
                    return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return NotFound("Service unavailable");
            }
        }
    }
}
