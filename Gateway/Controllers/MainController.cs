using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Authentication.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Gateway.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        private string AuthAddress;

        public MainController(IConfiguration configuration)
        {
            AuthAddress = configuration.GetSection("Addresses")["Auth"];
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
        public async Task<IActionResult> Login([FromForm] string name, [FromForm] string pswd)
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            using (var client = new HttpClient())
            {
                response = await client.PostAsync($"{AuthAddress}/login",
                    new StringContent(JsonConvert.SerializeObject(
                        new LoginModel { Username = name, Password = pswd }
                    ), Encoding.UTF8, "application/json"));
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await Authorize();
            }
            return Unauthorized();
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] string name, [FromForm] string pswd)
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            using (var client = new HttpClient())
            {
                response = await client.PostAsync($"{AuthAddress}/register",
                    new StringContent(JsonConvert.SerializeObject(
                        new UserModel { Username = name, Password = pswd }
                    ), Encoding.UTF8, "application/json"));
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await Authorize();
            }
            return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("news")]
        [Authorize]
        public async Task<List<string>> GetNews() // TODO: string -> news
        {
            return new List<string>();
        }

        [HttpPost]
        [Route("news")]
        public async Task<IActionResult> AddNews([FromBody] string text)
        {
            return Ok();
        }

        [HttpGet]
        [Route("subscribe")]
        public async Task<IActionResult> AddSubscription([FromQuery] string user)
        {
            return Ok();
        }

        [HttpDelete]
        [Route("subscribe")]
        public async Task<IActionResult> RemoveSubscription([FromQuery] string user)
        {
            return Ok();
        }

        private async Task<IActionResult> Authorize()
        {
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, Guid.NewGuid().ToString())
                }, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType)));
            return Ok();
        }
    }
}
