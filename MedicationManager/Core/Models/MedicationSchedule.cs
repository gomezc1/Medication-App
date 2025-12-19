using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Represents a scheduled dose of medication for a specific time
    /// </summary>
    [Table("MedicationSchedules")]
    public class MedicationSchedule
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to UserMedications
        /// </summary>
        [Required]
        [Column(TypeName = "INTEGER")]
        public int UserMedicationId { get; set; }

        /// <summary>
        /// Scheduled time stored as string in HH:mm format
        /// </summary>
        [Required]
        [Column(TypeName = "TEXT")]
        public string ScheduledTimeString { get; set; } = "08:00";

        /// <summary>
        /// Scheduled time as TimeSpan (not stored in database)
        /// </summary>
        [NotMapped]
        public TimeSpan ScheduledTime
        {
            get => TimeSpan.Parse(ScheduledTimeString);
            set => ScheduledTimeString = value.ToString(@"hh\:mm");
        }

        /// <summary>
        /// Time slot category
        /// </summary>
        [Required]
        [Column(TypeName = "TEXT")]
        public TimingPreference TimeSlot { get; set; }

        /// <summary>
        /// Amount to take at this time
        /// </summary>
        [Required]
        [Range(0.001, 1000)]
        [Column(TypeName = "REAL")]
        public decimal DoseAmount { get; set; }

        /// <summary>
        /// Unit for this dose
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column(TypeName = "TEXT")]
        public string DoseUnit { get; set; } = string.Empty;

        /// <summary>
        /// Special instructions for this dose
        /// </summary>
        [MaxLength(500)]
        [Column(TypeName = "TEXT")]
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// When this schedule was generated
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether this schedule entry is active
        /// </summary>
        [Column(TypeName = "INTEGER")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(UserMedicationId))]
        public virtual UserMedication UserMedication { get; set; } = null!;
    }
}