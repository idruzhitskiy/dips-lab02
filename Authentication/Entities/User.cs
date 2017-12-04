using Gateway.Extensions;
using Gateway.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Authentication.Entities
{
    public class User
    {
        public User() { }
        public User(UserModel model)
        {
            this.Name = model.Username;
            this.Password = model.Password.Sha256();
            this.Role = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
