using CommunityToolkit.Mvvm.ComponentModel;
using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Warnings;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MedicationScheduler.ViewModels
{
    /// <summary>
    /// View model for individual medication with warnings
    /// </summary>
    public partial class MedicationWarningViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _genericName = string.Empty;

        [ObservableProperty]
        private string _dosage = string.Empty;

        [ObservableProperty]
        private string _frequency = string.Empty;

        [ObservableProperty]
        private Brush _accentBrush = Brushes.Gray;

        // Actual detected issues
        public List<DrugInteraction> DrugInteractions { get; set; } = new();
        public List<DuplicateActiveIngredientWarning> DuplicateWarnings { get; set; } = new();

        // Static warning information (from FDA or database)
        public ObservableCollection<string> Contraindications { get; set; } = new();
        public ObservableCollection<string> WarningsPrecautions { get; set; } = new();
        public ObservableCollection<string> CommonInteractions { get; set; } = new();

        // Summary properties
        public bool HasDrugInteractions => DrugInteractions.Any();
        public bool HasDuplicateWarnings => DuplicateWarnings.Any();
        public int TotalWarnings => DrugInteractions.Count + DuplicateWarnings.Count;
    }
}
