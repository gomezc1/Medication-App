using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Saved collection of user medications for easy loading
    /// </summary>
    [Table("MedicationSets")]
    public class MedicationSet
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User-defined name for the medication set
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column(TypeName = "TEXT")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description
        /// </summary>
        [MaxLength(500)]
        [Column(TypeName = "TEXT")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// JSON serialized medication data
        /// </summary>
        [Required]
        [Column(TypeName = "TEXT")]
        public string MedicationData { get; set; } = string.Empty;

        /// <summary>
        /// When set was created
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// When set was last modified
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Deserialized medications (not stored in database)
        /// </summary>
        [NotMapped]
        public List<UserMedication> Medications { get; set; } = new();
    }
}