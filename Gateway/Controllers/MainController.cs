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

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserModel userModel)
        {
            var response = await accountsService.Register(userModel);
            logger.LogInformation($"Response from accounts service: {response?.StatusCode}");

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.LogInformation("Adding claim");
                //response = await accountsService.AddClaim(userModel.Username, await Authorize());
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
        
        [HttpGet("{name}/news")]
        [Authorize]
        public async Task<List<string>> GetNews(string name, int page = 0, int perpage = 0)
        {
            var userExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = name });
            logger.LogInformation($"User response: {userExists?.StatusCode}");

            if (userExists == null || userExists.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            List<string> response = await newsService.GetNewsForUser(name, page, perpage);
            if (response != null)
            {
                logger.LogInformation($"Number of news for user {name}: {response.Count}");
                return response;
            }
            else
            {
                logger.LogCritical("News service unavailable");
                return null;
            }
        }

        [HttpPost("news")]
        [Authorize]
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
        [Authorize]
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

        [HttpPost("{subscriber}/subscriptions")]
        [Authorize]
        public async Task<IActionResult> AddSubscription(string subscriber, AddSubscriptionModel addSubscriptionModel)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = subscriber });
            var authorExists = await accountsService.CheckIfUserExists(new ExistsModel { Username = addSubscriptionModel.Author });
            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}, author response: {authorExists?.StatusCode}");

            if (subscriberExists == null || authorExists == null)
                return NotFound("Accounts service unavailable");
            if (subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Subscriber doesn't exists");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return NotFound("Author doesn't exists");

            var response = await subscriptionsService.AddSubscription(subscriber, addSubscriptionModel.Author);
            if (response != null)
            {
                logger.LogInformation($"Attempt to add subscription {subscriber}-{addSubscriptionModel.Author}, response {response.StatusCode}");
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
        [Authorize]
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
