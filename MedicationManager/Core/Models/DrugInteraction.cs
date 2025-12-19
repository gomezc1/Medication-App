using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Represents known drug-to-drug interactions
    /// </summary>
    [Table("DrugInteractions")]
    public class DrugInteraction
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// RxCui of first drug in interaction
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string Drug1RxCui { get; set; } = string.Empty;

        /// <summary>
        /// RxCui of second drug in interaction
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string Drug2RxCui { get; set; } = string.Empty;

        /// <summary>
        /// Display name of first drug (populated at runtime)
        /// </summary>
        [MaxLength(200)]
        [Column(TypeName = "TEXT")]
        public string Drug1Name { get; set; } = string.Empty;

        /// <summary>
        /// Display name of second drug (populated at runtime)
        /// </summary>
        [MaxLength(200)]
        [Column(TypeName = "TEXT")]
        public string Drug2Name { get; set; } = string.Empty;

        /// <summary>
        /// Severity level of the interaction
        /// </summary>
        [Required]
        [Column(TypeName = "TEXT")]
        public InteractionSeverity SeverityLevel { get; set; }

        /// <summary>
        /// Type of interaction (Pharmacokinetic, Pharmacodynamic, etc.)
        /// </summary>
        [MaxLength(100)]
        [Column(TypeName = "TEXT")]
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// Description of what happens when drugs interact
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Column(TypeName = "TEXT")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Clinical recommendation for managing the interaction
        /// </summary>
        [MaxLength(1000)]
        [Column(TypeName = "TEXT")]
        public string Recommendation { get; set; } = string.Empty;

        /// <summary>
        /// Source of the interaction data
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column(TypeName = "TEXT")]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// When the source data was last updated
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime SourceDate { get; set; }

        /// <summary>
        /// When this record was created
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// When this record was last modified
        /// 
        [Column(TypeName = "TEXT")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}