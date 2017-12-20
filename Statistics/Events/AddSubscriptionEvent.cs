using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class AddSubscriptionEvent : Event
    {
        public string Author { get; set; }
        public string Subscriber { get; set; }
    }
}
