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

namespace Gateway.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        private AccountsService accountsService;
        private SubscriptionsService subscriptionsService;
        private NewsService newsService;

        public MainController(IConfiguration configuration)
        {
            var addresses = configuration.GetSection("Addresses");
            accountsService = new AccountsService(addresses["Accs"]);
            subscriptionsService = new SubscriptionsService(addresses["Subscriptions"]);
            newsService = new NewsService(addresses["News"]);
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

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await accountsService.AddClaim(loginModel.Username, await Authorize());
                return Ok();
            }
            return Unauthorized();
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserModel userModel)
        {
            var response = await accountsService.Register(userModel);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await accountsService.AddClaim(userModel.Username, await Authorize());
                return Ok();
            }
            return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await accountsService.RemoveClaim(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("name")]
        public async Task<string> GetName()
        {
            return await accountsService.GetNameByClaim(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet("news")]
        [Authorize]
        public async Task<List<string>> GetNews() // TODO: string -> news
        {
            var name = await GetName();
            return await newsService.GetNewsForUser(name);
        }

        [HttpPost]
        [Route("news")]
        public async Task<IActionResult> AddNews(NewsModel news)
        {
            var response = await newsService.AddNews(news);
            if (response.IsSuccessStatusCode)
                return Ok();
            else
                return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        [Route("subscribe/{author}")]
        public async Task<IActionResult> AddSubscription(string author)
        {
            var subscriber = await GetName();
            var response = await subscriptionsService.AddSubscription(subscriber, author);
            if(response.IsSuccessStatusCode)
                return Ok();
            else
                return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpDelete]
        [Route("subscribe/{author}")]
        public async Task<IActionResult> RemoveSubscription(string author)
        {
            var subscriber = await GetName();
            var response = await subscriptionsService.RemoveSubscription(subscriber, author);
            if (response.IsSuccessStatusCode)
                return Ok();
            else
                return BadRequest(await response.Content.ReadAsStringAsync());
        }

        private async Task<string> Authorize()
        {
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, Guid.NewGuid().ToString())
                }, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType)));
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
