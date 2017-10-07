using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Gateway.Models;
using Authentication.Entities;

namespace Authentication.Controllers
{
    [Route("api")]
    public class AccountsController : Controller
    {
        private ApplicationDbContext db;

        public AccountsController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Name == loginModel.Username && u.Password == loginModel.Password);
            if (user != null)
            {
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Name == userModel.Username && u.Password == userModel.Password);
            if (user == null)
            {
                await db.Users.AddAsync(new User(userModel));
                await db.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Duplicate");
        }

        [HttpGet("{name}")]
        public async Task<User> FindUser(string name)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.Name == name);
        }

        [HttpPost("claim")]
        public async Task<IActionResult> AddClaim(string name, string claim)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Name == name);
            if (user != null)
            {
                user.AddClaim(claim);
                db.Users.Update(user);
                db.SaveChanges();
                return Ok();
            }
            return BadRequest("User not found");
        }

        [HttpPut("claim")]
        public async Task<string> GetNameByClaim(string claim)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.ContainsClaim(claim));
            return user?.Name;
        }

        [HttpGet("claim/{claim}")]
        public async Task<IActionResult> RemoveClaim(string claim)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.ContainsClaim(claim));
            if (user != null)
            {
                user.RemoveClaim(claim);
                db.Users.Update(user);
                db.SaveChanges();
                return Ok();
            }
            return BadRequest("User not found");
        }
    }
}
