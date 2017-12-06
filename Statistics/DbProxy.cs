using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Statistics
{
    public class DbProxy
    {
        public DbProxy(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }
        public ApplicationDbContext DbContext { get; private set; }
    }
}
