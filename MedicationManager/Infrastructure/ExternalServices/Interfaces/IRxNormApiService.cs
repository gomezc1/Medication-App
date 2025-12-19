using MedicationManager.Infrastructure.ExternalServices.Models.RxNorm;

namespace MedicationManager.Infrastructure.ExternalServices.Interfaces
{
    public interface IRxNormApiService
    {
        Task<List<RxNormCandidate>> SearchApproximateMatchAsync(string term);
        Task<RxNormProperties?> GetRxCuiDetailsAsync(string rxCui);
        Task<List<string>> GetActiveIngredientsAsync(string rxCui);
        Task<List<string>> GetDrugClassesAsync(string rxCui);
        Task<List<RxNormConceptProperty>> GetRelatedDrugsAsync(string rxCui, string relationshipType = "ingredients");
    }
}