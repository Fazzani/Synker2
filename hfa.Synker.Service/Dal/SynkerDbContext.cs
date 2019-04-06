using hfa.synker.entities;
using hfa.synker.entities.Notifications;
using hfa.Synker.Service.Entities;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Service.Entities.Playlists;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Services.Dal
{
    public class SynkerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        public DbSet<Command> Command { get; set; }

        public DbSet<Playlist> Playlist { get; set; }

        public DbSet<Host> Hosts { get; set; }

        public DbSet<WebGrabConfigDocker> WebGrabConfigDockers { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasIndex(b => b.Email)
            .IsUnique();

            modelBuilder.Entity<Host>()
            .HasIndex(b => new { b.Address, b.Port });

            modelBuilder.Entity<Host>(entity =>
            {
                entity.OwnsOne(e => e.Authentication);
            });

            modelBuilder.Entity<Playlist>()
            .HasIndex(b => b.UniqueId)
            .IsUnique();

            modelBuilder.Entity<Playlist>().OwnsOne(x => x.SynkConfig);
            
            modelBuilder.Entity<WebGrabConfigDocker>().HasOne(r => r.RunnableHost).WithMany(x => x.WebGrabConfigDockers).HasForeignKey(x => x.HostId);
           
            base.OnModelCreating(modelBuilder);
        }

        public SynkerDbContext(DbContextOptions opt) : base(opt)
        {
            Database.SetCommandTimeout(15000);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            UpdateUpdatedProperty();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();
            UpdateUpdatedProperty();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateUpdatedProperty()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified) && e.Entity is EntityBase))
            {
                var entity = entry.Entity as EntityBase;
                entity.UpdatedDate = DateTime.UtcNow;
                if (entry.State == EntityState.Added)
                    entity.CreatedDate = DateTime.UtcNow;
            }
        }
    }
}
