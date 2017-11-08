using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class NewsModel
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
    }
}
