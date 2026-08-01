namespace Sharply.Domain.Models
{
    /// <summary>Resultado calculado del ranking de un grupo — no se persiste.</summary>
    public class LeaderboardEntry
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Xp { get; set; }
        public int Streak { get; set; }
        public int MissionsCompleted { get; set; }
    }
}
