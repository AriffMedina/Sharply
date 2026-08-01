using Sharply.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool DecayEmailEnabled { get; set; } = true;
        public double DecayRetentionThreshold { get; set; } = 0.5;
        public DecayStrategyType DecayStrategy { get; set; } = DecayStrategyType.Ebbinghaus;

        public int TotalXp { get; set; }
        public UserRole Role { get; set; } = UserRole.Member;

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
