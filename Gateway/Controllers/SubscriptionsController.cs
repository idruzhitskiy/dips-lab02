using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Subscriptions;
using Gateway.Services;
using Gateway.Models.Shared;
using Gateway.Pagination;

namespace Gateway.Controllers
{
    [Route("subscriptions")]
    public class SubscriptionsController : Controller
    {
        private ISubscriptionsService subscriptionsService;
        private IAccountsService accountsService;
        private GatewayController gatewayController;

        public SubscriptionsController(ISubscriptionsService subscriptionsService,
            IAccountsService accountsService,
            GatewayController gatewayController)
        {
            this.subscriptionsService = subscriptionsService;
            this.accountsService = accountsService;
            this.gatewayController = gatewayController;
        }

        [HttpGet]
        public async Task<IActionResult> Index(IndexModel indexModel)
        {
            if (ModelState.IsValid)
            {
                var authorsResponse = await gatewayController.GetSubscribedAuthors(indexModel.Username, indexModel.Page, indexModel.Size);
                if (authorsResponse.StatusCode == 200)
                {
                    PaginatedList<string> paginatedList = authorsResponse.Value as PaginatedList<string>;
                    indexModel.Authors = paginatedList.Content;
                    indexModel.Page = paginatedList.Page;
                    indexModel.MaxPage = paginatedList.MaxPage;
                    indexModel.Size = paginatedList.Size;
                    return View(indexModel);
                }
                else
                    return View("Error", new ErrorModel(authorsResponse));
            }
            else
                return RedirectToAction(nameof(Authenticate));
        }

        [HttpGet("authenticate")]
        public async Task<IActionResult> Authenticate()
        {
            return View();
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(string username, string author)
        {
            var result = await gatewayController.RemoveSubscription(username, author);
            if (result.StatusCode == 200)
                return RedirectToAction(nameof(Index), new IndexModel { Username = username });
            else
                return RedirectToAction("Error", new ErrorModel(result));
        }

        [HttpGet("add")]
        public async Task<IActionResult> Add(string username)
        {
            return View(new AddModel { Username = username });
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(AddModel addModel)
        {
            var result = await gatewayController.AddSubscription(addModel.Username, addModel.Author);
            if (result.StatusCode == 200)
                return RedirectToAction(nameof(Index), new IndexModel { Username = addModel.Username });
            else
                return View("Error", new ErrorModel(result));
        }
    }
}