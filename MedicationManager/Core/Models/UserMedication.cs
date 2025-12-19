using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Represents a medication that a user is taking with their specific dosing information
    /// </summary>
    public class UserMedication
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to Medications table
        /// </summary>
        [Required]
        [Column(TypeName = "INTEGER")]
        public int MedicationId { get; set; }

        /// <summary>
        /// User's prescribed or chosen dose amount
        /// </summary>
        [Required]
        [Range(0.001, 10000)]
        [Column(TypeName = "REAL")]
        public decimal UserDose { get; set; }

        /// <summary>
        /// Unit for the user's dose (tablets, mg, ml, etc.)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column(TypeName = "TEXT")]
        public string UserDoseUnit { get; set; } = string.Empty;

        /// <summary>
        /// How many times per day to take (1-8)
        /// </summary>
        [Required]
        [Range(1, 8)]
        [Column(TypeName = "INTEGER")]
        public int Frequency { get; set; }

        /// <summary>
        /// Preferred time slots for taking medication - stored as JSON
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string TimingPreferencesJson { get; set; } = "[]";

        /// <summary>
        /// Timing preferences list (not stored directly in database)
        /// </summary>
        [NotMapped]
        public List<TimingPreference> TimingPreferences
        {
            get => JsonSerializer.Deserialize<List<TimingPreference>>(TimingPreferencesJson) ?? new List<TimingPreference>();
            set => TimingPreferencesJson = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Specific times if user prefers exact scheduling - stored as JSON
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string SpecificTimesJson { get; set; } = "[]";

        /// <summary>
        /// Specific times list (not stored directly in database)
        /// </summary>
        [NotMapped]
        public List<TimeSpan> SpecificTimes
        {
            get
            {
                var timeStrings = JsonSerializer.Deserialize<List<string>>(SpecificTimesJson) ?? new List<string>();
                return timeStrings.Select(t => TimeSpan.Parse(t)).ToList();
            }
            set
            {
                var timeStrings = value.Select(t => t.ToString(@"hh\:mm")).ToList();
                SpecificTimesJson = JsonSerializer.Serialize(timeStrings);
            }
        }

        /// <summary>
        /// Must be taken with food
        /// </summary>
        [Column(TypeName = "INTEGER")]
        public bool WithFood { get; set; }

        /// <summary>
        /// Must be taken on empty stomach
        /// </summary>
        [Column(TypeName = "INTEGER")]
        public bool OnEmptyStomach { get; set; }

        /// <summary>
        /// Additional user instructions
        /// </summary>
        [MaxLength(500)]
        [Column(TypeName = "TEXT")]
        public string SpecialInstructions { get; set; } = string.Empty;

        /// <summary>
        /// Whether user is currently taking this medication
        /// </summary>
        [Column(TypeName = "INTEGER")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When user started taking this medication
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// When user stopped taking this medication
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// When record was created
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// When record was last modified
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Display string for timing preferences
        /// </summary>
        [NotMapped]
        public string TimingPreferencesDisplay
        {
            get
            {
                if (SpecificTimes != null && SpecificTimes.Any())
                    return string.Join(", ", SpecificTimes.Select(t => t.ToString(@"hh\:mm tt")));
                else if (TimingPreferences != null && TimingPreferences.Any())
                    return string.Join(", ", TimingPreferences);
                return "Not specified";
            }
        }

        // Navigation properties
        [ForeignKey(nameof(MedicationId))]
        public virtual Medication Medication { get; set; } = null!;

        public virtual ICollection<MedicationSchedule> Schedules { get; set; } = new List<MedicationSchedule>();
    }
}
