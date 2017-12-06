using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models.Admin
{
    public class RequestsModel
    {
        public List<Request> Requests { get; set; } = new List<Request>();
    }

    public class Request
    {
        public string From { get; set; }
        public int Count { get; set; }
        public DateTime Time { get; set; }
    }
}
