using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Tracks medications with duplicate active ingredients
    /// </summary>
    [Table("ActiveIngredientDuplicates")]
    public class ActiveIngredientDuplicate
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// RxCui of first medication
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string RxCui1 { get; set; } = string.Empty;

        /// <summary>
        /// RxCui of second medication
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string RxCui2 { get; set; } = string.Empty;

        /// <summary>
        /// The shared active ingredient
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column(TypeName = "TEXT")]
        public string SharedIngredient { get; set; } = string.Empty;

        /// <summary>
        /// Warning message about this duplication
        /// </summary>
        [MaxLength(500)]
        [Column(TypeName = "TEXT")]
        public string WarningMessage { get; set; } = string.Empty;

        /// <summary>
        /// When this record was created
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}