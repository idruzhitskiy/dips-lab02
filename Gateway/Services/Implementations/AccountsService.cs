using Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Services.Implementations
{
    public class AccountsService : Service, IAccountsService
    {
        public AccountsService(IConfiguration configuration) : 
            base(configuration.GetSection("Addresses")["Accs"]) { }

        public async Task<HttpResponseMessage> CheckIfUserExists(ExistsModel loginModel) => await PostJson("exists", loginModel);

        public async Task<HttpResponseMessage> Register(RegisterModel userModel) => await PostJson("register", userModel);

        public async Task<RegisterModel> DeleteUser(string username)
        {
            var httpResponseMessage = await Delete(username);
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<RegisterModel>(response);
            }
            catch
            {
                return null;
            }
        }
    }
}
