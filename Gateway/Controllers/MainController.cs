using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Gateway.Models;

namespace Gateway.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet("adduser")]
        public async Task<IActionResult> AddUser()
        {
            return View();
        }

        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser(UserModel userModel)
        {
            return Ok();
        }


        [HttpGet("addnews")]
        public async Task<IActionResult> AddNews()
        {
            return View(new NewsModel());
        }

        [HttpPost("addnews")]
        public async Task<IActionResult> AddNews(NewsModel newsModel)
        {
            if(ModelState.IsValid)
            {
                return Ok();
            }
            return NotFound();
        }

        [HttpGet("getnews")]
        public async Task<IActionResult> GetNews()
        {
            return View();
        }
    }
}