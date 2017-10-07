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
        public User(UserModel model)
        {
            this.Name = model.Username;
            this.Password = model.Password;
            this.Email = model.Email;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string StringClaims { get; set; }

        public void AddClaim(string claim)
        {
            StringClaims = $"{StringClaims}{claim};";
        }

        public void RemoveClaim(string claim)
        {
            if (ContainsClaim(claim))
                StringClaims = StringClaims.Remove(StringClaims.IndexOf(claim), claim.Length + 1);
        }

        public bool ContainsClaim(string claim)
        {
            return !string.IsNullOrWhiteSpace(claim) &&
                !string.IsNullOrWhiteSpace(StringClaims) &&
                StringClaims.IndexOf(claim) >= 0;
        }
    }
}
