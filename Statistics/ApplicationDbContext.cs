using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Statistics.Events;
using Statistics.Entities;

namespace Statistics
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<RequestInfo> Requests { get; set; }
        public DbSet<LoginInfo> Logins { get; set; }
        public DbSet<NewsAdditionInfo> NewsAdditions { get; set; }
        public DbSet<UserOperationInfo> UserOperations { get; set; }
        public DbSet<SubscriptionOperationInfo> SubscriptionOperations { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }
    }
}
