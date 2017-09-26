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

        public DbSet<Message> Messages { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
            
        //}

        public SynkerDbContext(DbContextOptions opt) : base(opt) {
            
        }
    }
}
