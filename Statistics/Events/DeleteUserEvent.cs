using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class DeleteUserEvent : Event
    {
        public string Username { get; set; }
    }
}
