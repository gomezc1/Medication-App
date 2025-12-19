using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Warnings;

namespace MedicationManager.Core.Services.Interfaces
{
    public interface IDosageValidationService
    {
        Task<List<DosageWarning>> ValidateAllMedicationsAsync(List<UserMedication> medications);
        Task<DosageWarning?> ValidateIndividualMedicationAsync(UserMedication medication);
    }
}