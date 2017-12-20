using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class UserOperationInfo
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public UserOperation Operation { get; set; }
        public DateTime Time { get; set; }
    }

    public enum UserOperation
    {
        Register,
        Delete,
        NameChanged
    }
}
