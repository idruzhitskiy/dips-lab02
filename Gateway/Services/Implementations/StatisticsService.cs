using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statistics.Models.Main;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Gateway.Services.Implementations
{
    public class StatisticsService : Service, IStatisticsService
    {
        public StatisticsService(IConfiguration configuration) : base(configuration.GetSection("Addresses")["Stat"]) { }

        public async Task<List<NewsAdditionModel>> GetNewsAdditions()
        {
            var res = await Get("newsadditions");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<NewsAdditionModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<NewsAdditionDetailModel>> GetNewsAdditionsDetailed()
        {
            var res = await Get("newsadditions/detail");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<NewsAdditionDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<OperationModel>> GetOperations()
        {
            var res = await Get("operations");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<OperationModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<OperationDetailModel>> GetOperationsDetailed()
        {
            var res = await Get("operations/detail");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<OperationDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<RequestModel>> GetRequests()
        {
            var res = await Get("requests");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<RequestModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<RequestDetailModel>> GetRequestsDetailed()
        {
            var res = await Get("requests/detail");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<RequestDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }
    }
}
