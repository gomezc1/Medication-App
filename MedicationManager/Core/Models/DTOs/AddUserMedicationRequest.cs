using System.ComponentModel.DataAnnotations;

namespace MedicationManager.Core.Models.DTOs
{
    /// <summary>
    /// Request object for adding a new user medication
    /// </summary>
    public class AddUserMedicationRequest
    {
        /// <summary>
        /// RxCui of the medication to add
        /// </summary>
        [Required(ErrorMessage = "Medication selection is required")]
        public string RxCui { get; set; } = string.Empty;

        /// <summary>
        /// Dose amount
        /// </summary>
        [Required]
        [Range(0.001, 10000, ErrorMessage = "Dose must be between 0.001 and 10000")]
        public decimal Dose { get; set; }

        /// <summary>
        /// Dose unit (tablets, mg, ml, etc.)
        /// </summary>
        [Required(ErrorMessage = "Dose unit is required")]
        [MaxLength(20)]
        public string DoseUnit { get; set; } = string.Empty;

        /// <summary>
        /// Times per day to take medication
        /// </summary>
        [Required]
        [Range(1, 8, ErrorMessage = "Frequency must be between 1 and 8 times per day")]
        public int Frequency { get; set; }

        /// <summary>
        /// Preferred timing slots
        /// </summary>
        public List<TimingPreference> TimingPreferences { get; set; } = new();

        /// <summary>
        /// Specific times if user prefers exact scheduling
        /// </summary>
        public List<TimeSpan> SpecificTimes { get; set; } = new();

        /// <summary>
        /// Must be taken with food
        /// </summary>
        public bool WithFood { get; set; }

        /// <summary>
        /// Must be taken on empty stomach
        /// </summary>
        public bool OnEmptyStomach { get; set; }

        /// <summary>
        /// Additional instructions
        /// </summary>
        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;

        /// <summary>
        /// When user started taking this medication
        /// </summary>
        public DateTime? StartDate { get; set; }
    }
}