using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[]
            {
                "POST: /login (user, pswd)",
                "GET: /logout",
                "GET: /news",
                "POST: /news (text)",
                "GET: /subscribe?user",
                "DELETE: /subscribe?user"
            };
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] string user, [FromBody] string pswd)
        {
            return Ok();
        }

        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok();
        }

        [HttpGet]
        [Route("news")]
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
    }
}
