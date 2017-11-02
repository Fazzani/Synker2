﻿using hfa.Synker.Service.Entities;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Service.Entities.Playlists;
using hfa.Synker.Services.Entities.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.Synker.Services.Dal
{
    public class SynkerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Command> Command { get; set; }
        public DbSet<Playlist> Playlist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasIndex(b => b.Email).IsUnique();

            modelBuilder.Entity<User>()
            .HasIndex(b => new { b.FirstName, b.LastName });

            modelBuilder.Entity<Playlist>().Property(x => x.Content).HasColumnType("JSON");

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
    }
}
