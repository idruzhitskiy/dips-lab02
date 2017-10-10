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
            HttpResponseMessage httpResponseMessage = await Get($"{name.ToLowerInvariant()}");
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(response);
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<HttpResponseMessage> AddNews(NewsModel newsModel)
        {
            return await PostJson("", newsModel);
        }
    }
}
