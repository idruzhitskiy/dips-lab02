using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Authentication.Controllers
{
    [Route("api")]
    public class AuthenticationController : Controller
    {
        private ApplicationDbContext db;

        public AuthenticationController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Name == loginModel.Username && u.Password == loginModel.Password);
            if (user != null)
            {
                await Authenticate(loginModel.Username);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Name == userModel.Username && u.Password == userModel.Password);
            if (user == null)
            {
                await db.Users.AddAsync(new User { Name = userModel.Username, Password = userModel.Password });
                await db.SaveChangesAsync();
                await Authenticate(userModel.Username);
                return Ok();
            }
            return BadRequest("Duplicate");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}
