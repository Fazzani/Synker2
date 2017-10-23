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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasIndex(b => b.Email).IsUnique();

            modelBuilder.Entity<User>()
            .HasIndex(b => new { b.FirstName, b.LastName });

            base.OnModelCreating(modelBuilder);
        }

        public SynkerDbContext(DbContextOptions opt) : base(opt)
        {
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            updateUpdatedProperty();

            return base.SaveChanges();
        }

        private void updateUpdatedProperty()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified && e.Entity is EntityBase))
            {
                var entity = entry.Entity as EntityBase;
                entity.UpdatedDate = DateTime.UtcNow;
                if (entry.State == EntityState.Added)
                    entity.CreatedDate = DateTime.UtcNow;
            }
        }

        public DbSet<hfa.WebApi.Dal.Entities.Command> Command { get; set; }
    }
}
