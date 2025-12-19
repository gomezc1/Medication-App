using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Represents a medication from external APIs or local database.
    /// Contains master medication information used across the application.
    /// </summary>
    public class Medication
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Primary key for local database
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]

        /// <summary>
        /// RxNorm Concept Unique Identifier - standardized drug identifier
        /// </summary>
        public string RxCui { get; set; } = string.Empty;

        /// <summary>
        /// Brand/trade name (e.g., "Tylenol")
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column(TypeName = "TEXT")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Generic drug name (e.g., "Acetaminophen")
        /// </summary>
        [MaxLength(200)]
        [Column(TypeName = "TEXT")]
        public string GenericName { get; set; } = string.Empty;

        /// <summary>
        /// List of active ingredients - stored as JSON in database
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string ActiveIngredientsJson { get; set; } = "[]";

        /// <summary>
        /// Active ingredients list (not stored directly in database)
        /// </summary>
        [NotMapped]
        public List<string> ActiveIngredients
        {
            get => JsonSerializer.Deserialize<List<string>>(ActiveIngredientsJson) ?? new List<string>();
            set => ActiveIngredientsJson = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Drug strength (e.g., "500mg", "10mg/5ml")
        /// </summary>
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string Strength { get; set; } = string.Empty;

        /// <summary>
        /// Dosage form (tablet, capsule, liquid, etc.)
        /// </summary>
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string DosageForm { get; set; } = string.Empty;

        /// <summary>
        /// Route of administration (oral, topical, etc.)
        /// </summary>
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// True if available over-the-counter
        /// </summary>
        [Column(TypeName = "INTEGER")]
        public bool IsOTC { get; set; }

        /// <summary>
        /// Maximum safe daily dose for OTC medications
        /// </summary>
        [Column(TypeName = "REAL")]
        public decimal? MaxDailyDose { get; set; }

        /// <summary>
        /// Unit for maximum daily dose (mg, ml, tablets, etc.)
        /// </summary>
        [MaxLength(20)]
        [Column(TypeName = "TEXT")]
        public string MaxDailyDoseUnit { get; set; } = string.Empty;

        /// <summary>
        /// Drug manufacturer
        /// </summary>
        [MaxLength(200)]
        [Column(TypeName = "TEXT")]
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// National Drug Code
        /// </summary>
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string NDC { get; set; } = string.Empty;

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
        /// Data source (OpenFDA, RxNorm, Manual)
        /// </summary>
        [MaxLength(50)]
        [Column(TypeName = "TEXT")]
        public string DataSource { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<UserMedication> UserMedications { get; set; } = new List<UserMedication>();
    }
}
