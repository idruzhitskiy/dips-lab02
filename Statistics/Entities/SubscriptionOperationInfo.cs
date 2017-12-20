using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class SubscriptionOperationInfo
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Subscriber { get; set; }
        public SubscriptionOperation Operation { get; set; }
        public DateTime Time { get; set; }
    }

    public enum SubscriptionOperation
    {
        Added,
        Removed
    }
}
