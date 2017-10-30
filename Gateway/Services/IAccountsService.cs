using Gateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface IAccountsService
    {
        Task<HttpResponseMessage> CheckIfUserExists(ExistsModel existsModel);
        Task<HttpResponseMessage> Register(RegisterModel userModel);
        Task<HttpResponseMessage> DeleteUser(string username);
    }
}
