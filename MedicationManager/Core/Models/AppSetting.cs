using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Application settings stored in database
    /// </summary>
    [Table("AppSettings")]
    public class AppSetting
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Setting key (unique identifier)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column(TypeName = "TEXT")]
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>
        /// Setting value (stored as string, cast based on SettingType)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string SettingValue { get; set; } = string.Empty;

        /// <summary>
        /// Type of setting (string, int, bool, json, datetime)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column(TypeName = "TEXT")]
        public string SettingType { get; set; } = "string";

        /// <summary>
        /// When setting was created
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// When setting was last modified
        /// </summary>
        [Column(TypeName = "TEXT")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}