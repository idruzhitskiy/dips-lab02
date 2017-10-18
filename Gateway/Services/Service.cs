using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public abstract class Service
    {
        private string baseAddress;

        public Service(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        protected async Task<HttpResponseMessage> PostJson(string addr, object obj)
        {
            using (var client = new HttpClient())
                try
                {
                    return await client.PostAsync(GetAddress(addr), new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> PostForm(string addr, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient())
                try
                {
                    return await client.PostAsync(GetAddress(addr), new FormUrlEncodedContent(parameters));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> PutForm(string addr, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient())
                try
                {
                    return await client.PutAsync(GetAddress(addr), new FormUrlEncodedContent(parameters));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> Get(string addr)
        {
            using (var client = new HttpClient())
                try
                {
                    return await client.GetAsync(GetAddress(addr));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> Delete(string addr)
        {
            using (var client = new HttpClient())
                try
                {
                    return await client.DeleteAsync(addr);
                }
                catch
                {
                    return null;
                }
        }

        private string GetAddress(string addr)
        {
            return $"{baseAddress}/{addr}";
        }
    }
}
