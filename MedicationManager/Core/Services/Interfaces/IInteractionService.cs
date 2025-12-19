using MedicationManager.Core.Models;
using MedicationManager.Core.Models.DTOs;
using MedicationManager.Core.Models.Warnings;

namespace MedicationManager.Core.Services.Interfaces
{
    public interface IInteractionService
    {
        Task<InteractionCheckResult> CheckInteractionsAsync(List<UserMedication> medications);
        Task<List<DrugInteraction>> CheckDrugToDrugInteractionsAsync(List<UserMedication> medications);
        Task<List<DuplicateActiveIngredientWarning>> CheckDuplicateIngredientsAsync(List<UserMedication> medications);
    }
}