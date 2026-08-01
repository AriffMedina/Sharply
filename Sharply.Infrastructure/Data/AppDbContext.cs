using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Enums;
using Sharply.Domain.Models;

namespace Sharply.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<SkillLog> SkillLogs => Set<SkillLog>();
        public DbSet<Mission> Missions => Set<Mission>();
        public DbSet<MissionCompletion> MissionCompletions => Set<MissionCompletion>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
        public DbSet<GroupSkill> GroupSkills => Set<GroupSkill>();

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
                .Property(s => s.Level)
                .HasConversion<string>();

            modelBuilder.Entity<Skill>()
                .Property(s => s.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.DecayEmailEnabled)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.DecayRetentionThreshold)
                .HasDefaultValue(0.5);

            modelBuilder.Entity<User>()
                .Property(u => u.DecayStrategy)
                .HasConversion<string>()
                .HasDefaultValue(Sharply.Domain.Enums.DecayStrategyType.Ebbinghaus);

            modelBuilder.Entity<MissionCompletion>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<MissionCompletion>()
                .HasOne(c => c.Mission)
                .WithMany()
                .HasForeignKey(c => c.MissionId);

            modelBuilder.Entity<Mission>()
                .Property(m => m.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Mission>()
                .Property(m => m.Period)
                .HasConversion<string>();

            modelBuilder.Entity<Mission>().HasData(
                new Mission
                {
                    Id = 1,
                    Title = "Practica 3 skills hoy",
                    Description = "Registra una sesion de practica en 3 skills distintas antes de que termine el dia.",
                    XpReward = 15,
                    Type = MissionType.DailyPractice,
                    Target = 3,
                    Period = MissionPeriod.Daily
                },
                new Mission
                {
                    Id = 2,
                    Title = "Rescata una skill en riesgo",
                    Description = "Practica una skill cuya retencion haya caido por debajo de tu umbral de alerta.",
                    XpReward = 25,
                    Type = MissionType.RescueRusty,
                    Target = 1,
                    Period = MissionPeriod.Daily
                },
                new Mission
                {
                    Id = 3,
                    Title = "Manten tu racha 7 dias",
                    Description = "Practica al menos una skill por dia durante 7 dias seguidos.",
                    XpReward = 50,
                    Type = MissionType.KeepStreak,
                    Target = 7,
                    Period = MissionPeriod.Weekly
                },
                new Mission
                {
                    Id = 4,
                    Title = "Practica al menos 1 skill hoy",
                    Description = "Registra una sesion de practica en cualquier skill antes de que termine el dia.",
                    XpReward = 5,
                    Type = MissionType.DailyPractice,
                    Target = 1,
                    Period = MissionPeriod.Daily
                },
                new Mission
                {
                    Id = 5,
                    Title = "Practica 5 skills hoy",
                    Description = "Registra una sesion de practica en 5 skills distintas en el mismo dia.",
                    XpReward = 30,
                    Type = MissionType.DailyPractice,
                    Target = 5,
                    Period = MissionPeriod.Daily
                },
                new Mission
                {
                    Id = 6,
                    Title = "Manten tu racha 3 dias",
                    Description = "Practica al menos una skill por dia durante 3 dias seguidos.",
                    XpReward = 20,
                    Type = MissionType.KeepStreak,
                    Target = 3,
                    Period = MissionPeriod.Weekly
                },
                new Mission
                {
                    Id = 7,
                    Title = "Manten tu racha 14 dias",
                    Description = "Practica al menos una skill por dia durante 14 dias seguidos.",
                    XpReward = 100,
                    Type = MissionType.KeepStreak,
                    Target = 14,
                    Period = MissionPeriod.Weekly
                },
                new Mission
                {
                    Id = 8,
                    Title = "Rescata una skill en riesgo esta semana",
                    Description = "Practica una skill cuya retencion haya caido por debajo de tu umbral de alerta, al menos una vez esta semana.",
                    XpReward = 40,
                    Type = MissionType.RescueRusty,
                    Target = 1,
                    Period = MissionPeriod.Weekly
                }
            );

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasDefaultValue(UserRole.Member);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Owner)
                .WithMany()
                .HasForeignKey(g => g.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasIndex(g => g.InviteCode)
                .IsUnique();

            modelBuilder.Entity<GroupMember>()
                .HasOne(m => m.Group)
                .WithMany()
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupSkill>()
                .HasOne(gs => gs.Group)
                .WithMany()
                .HasForeignKey(gs => gs.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupSkill>()
                .Property(gs => gs.Level)
                .HasConversion<string>();

            modelBuilder.Entity<GroupSkill>()
                .Property(gs => gs.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Skill>()
                .HasOne<Group>()
                .WithMany()
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
