using Authentication.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public ApplicationDbContext() : base()
        {
            Initialize();
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> ops) : base(ops)
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Users.Any())
            {
                Users.Add(new User { Name = "User1" });
                Users.Add(new User { Name = "User2" });
                Users.Add(new User { Name = "User3" });
                SaveChanges();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
        }
    }
}
