using Sharply.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sharply.Web.ViewModels
{
    public class SkillFormViewModel
    {
        [Required(ErrorMessage = "Give your skill a name")]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        [StringLength(280)]
        public string? Description { get; set; }

        public Level Level { get; set; } = Level.Intermediate;
    }
}
