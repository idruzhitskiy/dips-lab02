using Microsoft.AspNetCore.Mvc;
using Statistics.Entities;
using Statistics.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Controllers
{
    public class MainController : Controller
    {
        private ApplicationDbContext dbContext;
        public MainController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("newsadditions")]
        public async Task<List<NewsAdditionModel>> GetNewsAdditions()
        {
            return dbContext.NewsAdditions
                .Select(nai => nai.Author)
                .GroupBy(s => s)
                .Select(g => new NewsAdditionModel { Author = g.Key, Count = g.Count() })
                .ToList();
        }

        [HttpGet("newsadditions/detail")]
        public async Task<List<NewsAdditionDetailModel>> GetNewsAdditionsDetailed()
        {
            return dbContext.NewsAdditions.Select(i => new NewsAdditionDetailModel
            {
                Author = i.Author,
                Time = i.AddedTime.ToString(@"ddMM-HH:mm")
            }).ToList();
        }

        [HttpGet("requests")]
        public async Task<List<RequestModel>> GetRequests(int forSeconds = 60)
        {
            var requests = dbContext.Requests.Where(i => i.RequestType == Events.RequestType.Gateway).ToList();
            var minDate = DateTime.Now - TimeSpan.FromSeconds(forSeconds);
            var maxDate = DateTime.Now;
            int numOfIntervals = 10;
            var span = (maxDate - minDate) / numOfIntervals;
            var intervaledRequests = new Dictionary<DateTime, List<RequestInfo>>();
            for (int i = 0; i < numOfIntervals; i++)
            {
                intervaledRequests.Add(minDate + 0.5 * span + span * i, new List<RequestInfo> { new RequestInfo { Time = minDate + 0.5 * span + span * i, From = null } });
            }
            foreach (var request in requests)
            {
                foreach (var key in intervaledRequests.Keys)
                    if ((key - request.Time).Duration() < (0.5 * span))
                    {
                        intervaledRequests[key].Add(request);
                        break;
                    }
            }
            var test = intervaledRequests.Values.Select(v => v.Count()).ToList();
            var diffconst = 0.5 * (forSeconds / numOfIntervals);
            return intervaledRequests
                .SelectMany(kv => kv.Value.GroupBy(i => i.From).Select(g => (kv.Key, g.Key, g.ToList())))
                .Select(t => new RequestModel
                {
                    Count = t.Item2 == null ? 0 : t.Item3.Count,
                    From = t.Item2,
                    Time = $"{((t.Item1 - maxDate).Seconds - diffconst).ToString()}s - {((t.Item1 - maxDate).Seconds + diffconst).ToString()}s"
                }).ToList();
        }

        [HttpGet("requests/detail")]
        public async Task<List<RequestDetailModel>> GetRequestsDetailed()
        {
            return dbContext.Requests.Select(i => new RequestDetailModel { From = i.From, Time = i.Time, To = i.To }).ToList();
        }

        [HttpGet("operations/detail")]
        public async Task<List<OperationDetailModel>> GetOperationsDetailed()
        {
            return dbContext.UserOperations.Select(i => new OperationDetailModel { Operation = Enum.GetName(typeof(UserOperation), i.Operation), Subject = i.Subject, Time = i.Time }).ToList();
        }

        [HttpGet("operations")]
        public async Task<List<OperationModel>> GetOperations()
        {
            return dbContext.UserOperations.GroupBy(i => i.Operation).Select(g => new OperationModel
            {
                Type = Enum.GetName(typeof(UserOperation), g.Key),
                Count = g.Count()
            }).ToList();
        }
    }
}
