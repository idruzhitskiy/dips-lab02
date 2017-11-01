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
        Task<List<string>> GetNewsByUser(string username, int page, int perpage);
        Task<List<NewsModel>> DeleteNewsWithAuthor(string username);
        Task<HttpResponseMessage> ChangeUserName(string username, string newUsername);
        Task<List<string>> GetNews();
    }
}
