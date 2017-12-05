using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class OperationInfo
    {
        public int Id { get; set; }
        public string Object { get; set; }
        public string Subject { get; set; }
        public DateTime Time { get; set; }
    }
}
