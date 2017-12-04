using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class AccessInfo
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Time { get; set; }
    }
}
