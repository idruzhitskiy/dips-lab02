﻿using Gateway.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services.Implementations
{
    public class NewsService : Service, INewsService
    {
        public NewsService(IConfiguration configuration) 
            : base(configuration.GetSection("Addresses")["News"]) { }

        public async Task<List<string>> GetNewsForUser(string name, int page, int perpage)
        {
            var httpResponseMessage = await Get($"{name.ToLowerInvariant()}?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddNews(NewsModel newsModel) => await PostJson("", newsModel);
    }
}