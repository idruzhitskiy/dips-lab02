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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var response = await accountsService.Login(loginModel);
            logger.LogInformation($"Response from accounts service: {response?.StatusCode}");

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.LogInformation("Adding claim");
                response = await accountsService.AddClaim(loginModel.Username, await Authorize());
                logger.LogInformation($"Adding claim response: {response?.StatusCode}");
                return Ok();
            }
            else if (response != null)
            {
                logger.LogWarning($"User {loginModel.Username} not authorized");
                return Unauthorized();
            }
            else
            {
                logger.LogCritical("Accounts service unavailable");
                return NotFound("Service unavailable");
            }
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
                response = await accountsService.AddClaim(userModel.Username, await Authorize());
                logger.LogInformation($"Adding claim response: {response?.StatusCode}");
                return Ok();
            }
            else if (response != null)
            {
                string respContent = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                logger.LogError($"User {userModel.Username} not registered, error content: {respContent}");
                return Unauthorized();
            }
            else
            {
                logger.LogCritical("Accounts service unavailable");
                return NotFound("Service unavailable");
            }
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var response = await accountsService.RemoveClaim(await GetCurrentUsername());
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (response != null)
            {
                logger.LogInformation($"Attempt to remove claim from users database, result: {response.StatusCode}");
                return Ok();
            }
            else
            {
                logger.LogCritical("Accounts service unavailable");
                return NotFound("Service unavailable");
            }
        }

        [HttpGet("news")]
        [Authorize]
        public async Task<List<string>> GetNews(int page = 0, int perpage = 0)
        {
            var name = await GetCurrentUsername();
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
            news.Author = await GetCurrentUsername();
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

        [HttpGet("subscriptions")]
        [Authorize]
        public async Task<List<string>> GetSubscribedAuthors(int page = 0, int perpage = 0)
        {
            var user = await GetCurrentUsername();
            logger.LogInformation($"Requesting authors for user {user}");
            var authors = await subscriptionsService.GetSubscribedAuthorsForName(user, page, perpage);
            if (authors != null)
            {
                logger.LogInformation($"Found {authors.Count()} authors for user {user}");
                return authors;
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return null;
            }
        }

        [HttpPost("subscriptions")]
        [Authorize]
        public async Task<IActionResult> AddSubscription(AddSubscriptionModel addSubscriptionModel)
        {
            var subscriber = await GetCurrentUsername();
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

        [HttpDelete("subscriptions/{author}")]
        [Authorize]
        public async Task<IActionResult> RemoveSubscription(string author)
        {
            var subscriber = await GetCurrentUsername();
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

        private async Task<string> Authorize()
        {
            ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, Guid.NewGuid().ToString())
                }, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            if (HttpContext != null)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return identity.Name;
        }

        private string GetCurrentCookie()
        {
            string cookie = User?.FindFirstValue(ClaimTypes.Name);
            logger.LogInformation($"Requested current cookie, value: {cookie}");
            return cookie;
        }

        private async Task<string> GetCurrentUsername()
        {
            string name = await accountsService.GetNameByClaim(GetCurrentCookie());
            logger.LogInformation($"Requested current username, value: {name}");
            return name;
        }
    }
}
