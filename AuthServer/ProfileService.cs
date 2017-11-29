﻿using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using System.Security.Claims;
using IdentityServer4.Extensions;

namespace AuthServer
{
    public class ProfileService : IProfileService
    {
        private ApplicationDbContext dbContext;

        public ProfileService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var id = int.Parse(context.Subject.GetSubjectId());
            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
                context.AddRequestedClaims(new List<Claim> { new Claim("Name", user.Username) });
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var id = int.Parse(context.Subject.GetSubjectId());
            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);
            context.IsActive = user != null;
        }
    }
}
