using Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public class AccountsService : Service
    {
        public AccountsService(string address) : base(address) { }

        public async Task<HttpResponseMessage> Login(LoginModel loginModel) => await PostJson("login", loginModel);

        public async Task<HttpResponseMessage> Register(UserModel userModel) => await PostJson("register", userModel);

        public async Task<HttpResponseMessage> AddClaim(string name, string claim)
        {
            return await PostForm("claim", new Dictionary<string, string> { { "name", name }, { "claim", claim } });
        }

        public async Task<string> GetNameByClaim(string claim)
        {
            return await (await PutForm("claim", new Dictionary<string, string> { { "claim", claim } }))
                .Content
                .ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> RemoveClaim(string claim) => await Get("claim/{claim}");
    }
}
