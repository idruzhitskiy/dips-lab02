using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Shared;
using System.Text.RegularExpressions;
using Gateway.Models.Users;

namespace Gateway.Controllers
{
    [Route("users")]
    public class UsersController : Controller
    {
        private GatewayController gatewayController;

        public UsersController(GatewayController gatewayController)
        {
            this.gatewayController = gatewayController;
        }
        public async Task<IActionResult> Index(IndexModel indexModel)
        {
            if (ModelState.IsValid)
                return View(indexModel);
            else
                return RedirectToAction(nameof(Authenticate));
        }

        [HttpGet("authenticate")]
        public async Task<IActionResult> Authenticate()
        {
            return View();
        }

        [HttpGet("register")]
        public async Task<IActionResult> Register(string username)
        {
            var result = await gatewayController.Register(new Models.UserModel { Username = username });
            if (result.StatusCode != 200)
                return View("Error", new ErrorModel(result));
            return RedirectToAction(nameof(Index), new IndexModel { Username = username });
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string username)
        {
            var result = await gatewayController.CheckIfUserExist(new Models.UserModel { Username = username });
            if (result.StatusCode != 200)
                return View("Error", new ErrorModel(result));
            return RedirectToAction(nameof(Index), new IndexModel { Username = username });
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(string username, bool submit = false)
        {
            if (submit)
            {
                var result = await gatewayController.DeleteUser(username);
                if (result.StatusCode == 200)
                    return RedirectToAction(nameof(Index));
                return View("Error", new ErrorModel(result));
            }
            return View(nameof(Delete), username);
        }

        [HttpGet("change")]
        public async Task<IActionResult> Change(string username, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                return View(nameof(Change), username);

            var result = await gatewayController.ChangeUserName(username, newUsername);
            if (result.StatusCode == 200)
                return RedirectToAction(nameof(Index), new IndexModel { Username = newUsername });
            return View("Error", new ErrorModel(result));
        }
    }
}