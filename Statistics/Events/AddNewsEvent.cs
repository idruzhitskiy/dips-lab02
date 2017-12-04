using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class AddNewsEvent : Event
    {
        public string Author { get; set; }
    }
}
