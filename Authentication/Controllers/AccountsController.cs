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
using Microsoft.Extensions.Logging;

namespace Authentication.Controllers
{
    [Route("api")]
    public class AccountsController : Controller
    {
        private ApplicationDbContext db;
        private ILogger<AccountsController> logger;

        public AccountsController(ApplicationDbContext db, ILogger<AccountsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            logger.LogDebug($"Login request, username: {loginModel.Username}");
            User user = await db.Users.FirstOrDefaultAsync(u => u.Name == loginModel.Username && u.Password == loginModel.Password);
            if (user != null)
            {
                logger.LogDebug($"User {user.Name} found");
                return Ok();
            }
            logger.LogWarning($"User {loginModel.Username} not found");
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            logger.LogDebug($"Register request, username: {userModel.Username}");
            User user = await db.Users.FirstOrDefaultAsync(u => u.Name == userModel.Username && u.Password == userModel.Password);
            if (user == null)
            {
                logger.LogDebug($"Registering user {userModel.Username}");
                var result = await db.Users.AddAsync(new User(userModel));
                logger.LogDebug($"User {userModel.Username} add result: {result.State}");
                await db.SaveChangesAsync();
                return Ok();
            }
            logger.LogWarning($"User {userModel.Username} already exists");
            return BadRequest("Duplicate");
        }

        [HttpPost("claim")]
        public async Task<IActionResult> AddClaim(string name, string claim)
        {
            logger.LogDebug($"Adding claim {claim} to user {name}");
            var user = await db.Users.FirstOrDefaultAsync(u => u.Name == name);
            if (user != null)
            {
                user.AddClaim(claim);
                var result = db.Users.Update(user);
                logger.LogDebug($"User {name} modification result: {result.State}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"User {name} not found");
            return BadRequest("User not found");
        }

        [HttpPut("claim")]
        public async Task<string> GetNameByClaim(string claim)
        {
            logger.LogDebug($"Retrieving name for claim {claim}");
            var user = await db.Users.FirstOrDefaultAsync(u => u.ContainsClaim(claim));
            if (user == null)
                logger.LogWarning($"User with claim {claim} not found");
            else
                logger.LogDebug($"Found user {user.Name}");
            return user?.Name;
        }

        [HttpGet("claim/{claim}")]
        public async Task<IActionResult> RemoveClaim(string claim)
        {
            logger.LogDebug($"Removing claim {claim}");
            var user = await db.Users.FirstOrDefaultAsync(u => u.ContainsClaim(claim));
            if (user != null)
            {
                user.RemoveClaim(claim);
                var result = db.Users.Update(user);
                logger.LogDebug($"User {user.Name} modification result: {result.State}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"User with claim {claim} not found");
            return BadRequest("User not found");
        }
    }
}
