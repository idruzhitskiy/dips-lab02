using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Controllers
{
    public class MainController : Controller
    {
        private ApplicationDbContext dbContext;
        public MainController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("accesses")]
        public async Task<ObjectResult> GetAccesses()
        {
            return StatusCode(200,  dbContext.Accesses.ToList());
        }
    }
}
