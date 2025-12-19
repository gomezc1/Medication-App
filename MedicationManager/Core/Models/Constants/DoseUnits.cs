namespace MedicationManager.Core.Models.Constants
{
    /// <summary>
    /// Standard dose units supported by the application
    /// </summary>
    public static class DoseUnits
    {
        public const string Tablet = "tablet";
        public const string Capsule = "capsule";
        public const string Milliliter = "ml";
        public const string Milligram = "mg";
        public const string Gram = "g";
        public const string Teaspoon = "tsp";
        public const string Tablespoon = "tbsp";
        public const string Drop = "drop";
        public const string Spray = "spray";
        public const string Patch = "patch";
        public const string Unit = "unit";

        /// <summary>
        /// All supported dose units
        /// </summary>
        public static readonly List<string> All =
        [
            Tablet,
            Capsule,
            Milliliter,
            Milligram,
            Gram,
            Teaspoon,
            Tablespoon,
            Drop,
            Spray,
            Patch,
            Unit
        ];

        /// <summary>
        /// Solid dose forms
        /// </summary>
        public static readonly List<string> SolidForms =
        [
            Tablet,
            Capsule
        ];

        /// <summary>
        /// Liquid volume units
        /// </summary>
        public static readonly List<string> LiquidUnits =
        [
            Milliliter,
            Teaspoon,
            Tablespoon,
            Drop
        ];

        /// <summary>
        /// Weight units
        /// </summary>
        public static readonly List<string> WeightUnits =
        [
            Milligram,
            Gram
        ];
    }
}