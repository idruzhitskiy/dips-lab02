using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class UserModel
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
