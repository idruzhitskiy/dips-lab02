using Microsoft.EntityFrameworkCore;
using NewsStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsStorage
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<News> News { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> ops) : base(ops) { }
    }
}
