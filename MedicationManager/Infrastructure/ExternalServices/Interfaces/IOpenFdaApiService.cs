using MedicationManager.Core.Models;
using MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA;

namespace MedicationManager.Infrastructure.ExternalServices.Interfaces
{
    public interface IOpenFdaApiService
    {
        Task<FdaDrugResponse> SearchDrugsAsync(string searchTerm, int limit = 10);
        Task<FdaDrugResponse> SearchByRxCuiAsync(string rxCui);
        Task<List<DrugInteraction>> GetDrugInteractionsAsync(string rxCui);
        Task<List<DrugInteraction>> GetDrugInteractionsByNameAsync(string drugName1, string drugName2);
        Task<FdaDrugResult?> GetDrugLabelAsync(string ndc);
    }
}