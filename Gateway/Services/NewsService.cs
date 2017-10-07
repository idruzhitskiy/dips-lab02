using Gateway.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public class NewsService : Service
    {
        public NewsService(string baseAddress) : base(baseAddress) { }

        public async Task<List<string>> GetNewsForUser(string name)
        {
            return JsonConvert.DeserializeObject<List<string>>(await (await Get($"{name}")).Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> AddNews(NewsModel newsModel)
        {
            return await PostJson("", newsModel);
        }
    }
}
