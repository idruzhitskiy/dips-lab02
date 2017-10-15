using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public class SubscriptionsService : Service
    {
        public SubscriptionsService(string address) : base(address) { }

        public async Task<List<string>> GetSubscribedAuthorsForName(string name, int page, int perpage)
        {
            return JsonConvert.DeserializeObject<List<string>>(await (await Get($"{name}?page={page}&perpage={perpage}")).Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> AddSubscription(string subscriber, string author)
        {
            return await PostForm($"{subscriber}", new Dictionary<string, string> { { "author", author } });
        }

        public async Task<HttpResponseMessage> RemoveSubscription(string subscriber, string author)
        {
            return await Delete($"{subscriber}/{author}");
        }
    }
}
