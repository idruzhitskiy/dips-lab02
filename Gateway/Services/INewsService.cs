using Gateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface INewsService
    {
        Task<List<string>> GetNewsForUser(string name, int page, int perpage);
        Task<HttpResponseMessage> AddNews(NewsModel newsModel);
    }
}
