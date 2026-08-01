namespace Sharply.Domain.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int OwnerUserId { get; set; }
        public User Owner { get; set; } = null!;

        public string InviteCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
