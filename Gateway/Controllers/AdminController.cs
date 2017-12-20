using Gateway.Models.Admin;
using Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Statistics.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Controllers
{
    [Route("admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private IStatisticsService statisticsService;

        public AdminController(IStatisticsService statisticsService)
        {
            this.statisticsService = statisticsService;
        }

        [HttpGet("requests")]
        public async Task<IActionResult> RequestsStatistic()
        {
            return View();
        }

        [HttpGet("requests/values")]
        public async Task<List<RequestModel>> Requests()
        {
            return await statisticsService.GetRequests();
        }

        [HttpGet("requests/values/detail")]
        public async Task<List<RequestDetailModel>> RequestsDetail()
        {
            return await statisticsService.GetRequestsDetailed();
        }

        [HttpGet("news/added")]
        public async Task<IActionResult> NewsAddedStatistics()
        {
            return View();
        }

        [HttpGet("news/added/value")]
        public async Task<List<NewsAdditionModel>> NewsAddedValue()
        {
            return await statisticsService.GetNewsAdditions();
        }
        
        [HttpGet("news/added/detail")]
        public async Task<List<NewsAdditionDetailModel>> NewsAddedDetail()
        {
            return await statisticsService.GetNewsAdditionsDetailed();
        }

        public async Task<IActionResult> OperationsStatistics()
        {
            return View();
        }

        [HttpGet("operaionts/detail")]
        public async Task<List<OperationDetailModel>> OperationsDetail()
        {
            return await statisticsService.GetOperationsDetailed();
        }

        [HttpGet("operations/value")]
        public async Task<List<OperationModel>> OperationsValue()
        {
            return await statisticsService.GetOperations();
        }
    }
}
