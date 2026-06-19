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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
