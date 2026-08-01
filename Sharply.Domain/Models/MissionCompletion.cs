namespace Sharply.Domain.Models
{
    public class MissionCompletion
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        public int XpAwarded { get; set; }
    }
}
