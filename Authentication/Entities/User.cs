using Gateway.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Entities
{
    public class User
    {
        public User() { }
        public User(RegisterModel model)
        {
            this.Name = model.Username;
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
