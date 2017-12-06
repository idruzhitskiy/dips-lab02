using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class ChangeUsernameEvent : Event
    {
        public string OldUsername { get; set; }
        public string NewUsername { get; set; }
    }
}
