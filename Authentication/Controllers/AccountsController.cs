﻿using System;
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

        [HttpPost("exists")]
        public async Task<IActionResult> CheckIfUserExists([FromBody] ExistsModel existsModel)
        {
            logger.LogDebug($"Login request, username: {existsModel.Username}");
            User user = db.Users.FirstOrDefault(u => u.Name == existsModel.Username);
            if (user != null)
            {
                logger.LogDebug($"User {user.Name} found");
                return Ok();
            }
            logger.LogWarning($"User {existsModel.Username} not found");
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            logger.LogDebug($"Register request, username: {userModel.Username}");
            User user = db.Users.FirstOrDefault(u => u.Name == userModel.Username && u.Password == userModel.Password);
            if (user == null)
            {
                logger.LogDebug($"Registering user {userModel.Username}");
                var result = db.Users.Add(new User(userModel));
                logger.LogDebug($"User {userModel.Username} add result: {result?.State}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"User {userModel.Username} already exists");
            return BadRequest("Duplicate");
        }
    }
}
