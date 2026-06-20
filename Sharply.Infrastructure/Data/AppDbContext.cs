using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Models;

namespace Sharply.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<SkillLog> SkillLogs => Set<SkillLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Skill>()
                .HasOne(s => s.User)
                .WithMany(u => u.Skills)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<SkillLog>()
                .HasOne(l => l.Skill)
                .WithMany(s => s.Logs)
                .HasForeignKey(l => l.SkillId);

            modelBuilder.Entity<Skill>()
                .Property(s => s.MasteryLevel)
                .HasConversion<string>();

            modelBuilder.Entity<Skill>()
                .Property(s => s.Priority)
                .HasConversion<string>();
        }
    }
}
