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
        Task<HttpResponseMessage> Register(UserModel userModel);
        Task<UserModel> DeleteUser(string username);
        Task<HttpResponseMessage> ChangeUserName(string username, string newUsername);
    }
}
