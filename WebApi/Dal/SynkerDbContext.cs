using hfa.WebApi.Dal.Entities;
using Hfa.SyncLibrary.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal
{
    public class SynkerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Message { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseMySql(@"Server=150.80.235.155;database=playlist;uid=pl;pwd=pl;");
    }
}
