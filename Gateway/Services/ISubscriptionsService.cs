using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface ISubscriptionsService
    {
        Task<List<string>> GetSubscribedAuthorsForName(string name, int page, int perpage);
        Task<HttpResponseMessage> AddSubscription(string subscriber, string author);
        Task<HttpResponseMessage> RemoveSubscription(string subscriber, string author);
        Task<List<string>> GetAllAssociatedSubscriptions(string username, int page, int perpage);
        Task<HttpResponseMessage> RemoveAllAssociatedSubscriptions(string username);
    }
}
