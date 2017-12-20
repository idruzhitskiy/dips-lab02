using Statistics.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface IStatisticsService
    {
        Task<List<NewsAdditionModel>> GetNewsAdditions();
        Task<List<NewsAdditionDetailModel>> GetNewsAdditionsDetailed();
        Task<List<RequestModel>> GetRequests();
        Task<List<RequestDetailModel>> GetRequestsDetailed();
        Task<List<OperationModel>> GetOperations();
        Task<List<OperationDetailModel>> GetOperationsDetailed();
    }
}
