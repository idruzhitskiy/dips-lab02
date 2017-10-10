﻿using System;
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

namespace Gateway.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        private AccountsService accountsService;
        private SubscriptionsService subscriptionsService;
        private NewsService newsService;
        private ILogger<MainController> logger;

        public MainController(IConfiguration configuration, ILogger<MainController> logger)
        {
            this.logger = logger;
            var addresses = configuration.GetSection("Addresses");
            accountsService = new AccountsService(addresses["Accs"]);
            subscriptionsService = new SubscriptionsService(addresses["Subscriptions"]);
            newsService = new NewsService(addresses["News"]);
            logger.LogInformation("Controller init successful");
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[]
            {
                "POST: /login (email, pswd)",
                "POST: /register (email, pswd)",
                "GET: /logout",
                "GET: /news",
                "POST: /news (text)",
                "GET: /subscribe?user",
                "DELETE: /subscribe?user"
            };
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var response = await accountsService.Login(loginModel);
            logger.LogInformation($"Response from accounts service: {response.StatusCode}");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.LogInformation("Adding claim");
                response = await accountsService.AddClaim(loginModel.Username, await Authorize());
                logger.LogInformation($"Adding claim response: {response.StatusCode}");
                return Ok();
            }
            logger.LogWarning($"User {loginModel.Username} not authorized");
            return Unauthorized();
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserModel userModel)
        {
            var response = await accountsService.Register(userModel);
            logger.LogInformation($"Response from accounts service: {response.StatusCode}");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.LogInformation("Adding claim");
                response = await accountsService.AddClaim(userModel.Username, await Authorize());
                logger.LogInformation($"Adding claim response: {response.StatusCode}");
                return Ok();
            }
            logger.LogError($"User {userModel.Username} not registered, error content: {await response.Content.ReadAsStringAsync()}");
            return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var response = await accountsService.RemoveClaim(await GetCurrentUsername());
            logger.LogInformation($"Attempt to remove claim from users database, result: {response.StatusCode}");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("news")]
        [Authorize]
        public async Task<List<string>> GetNews() // TODO: string -> news
        {
            var name = await GetCurrentUsername();
            List<string> response = await newsService.GetNewsForUser(name);
            logger.LogInformation($"Number of news for user {name}: {response.Count}");
            return response;
        }

        [HttpPost]
        [Route("news")]
        public async Task<IActionResult> AddNews(NewsModel news)
        {
            news.Author = await GetCurrentUsername();
            var response = await newsService.AddNews(news);
            logger.LogInformation($"Attempt to add news, response {response.StatusCode}");
            if (response.IsSuccessStatusCode)
                return Ok();
            else
                return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        [Route("subscribe/{author}")]
        public async Task<IActionResult> AddSubscription(string author)
        {
            var subscriber = await GetCurrentUsername();
            var response = await subscriptionsService.AddSubscription(subscriber, author);
            logger.LogInformation($"Attempt to add subscription, response {response.StatusCode}");

            if (response.IsSuccessStatusCode)
                return Ok();
            else
                return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpDelete]
        [Route("subscribe/{author}")]
        public async Task<IActionResult> RemoveSubscription(string author)
        {
            var subscriber = await GetCurrentUsername();
            var response = await subscriptionsService.RemoveSubscription(subscriber, author);
            logger.LogInformation($"Attempt to remove subscription, response {response.StatusCode}");

            if (response.IsSuccessStatusCode)
                return Ok();
            else
                return BadRequest(await response.Content.ReadAsStringAsync());
        }

        private async Task<string> Authorize()
        {
            ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, Guid.NewGuid().ToString())
                }, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return identity.Name;
        }

        private string GetCurrentCookie()
        {
            string cookie = User.FindFirstValue(ClaimTypes.Name);
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
