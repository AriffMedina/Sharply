using Sharply.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sharply.Web.ViewModels
{
    public class SettingsViewModel
    {
        [Required(ErrorMessage = "Your name can't be empty")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool DecayEmailEnabled { get; set; } = true;

        [Range(0.0, 1.0, ErrorMessage = "Threshold must be between 0 and 1")]
        public double DecayRetentionThreshold { get; set; } = 0.5;

        public DecayStrategyType DecayStrategy { get; set; } = DecayStrategyType.Ebbinghaus;
    }
}
