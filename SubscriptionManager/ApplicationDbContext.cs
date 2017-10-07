using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionManager
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> ops) : base(ops) { }
    }
}
