using MedicationManager.Core.Models;
using MedicationManager.Core.Models.DTOs;

namespace MedicationManager.Core.Services.Interfaces
{
    public interface IMedicationService
    {
        // Search operations
        Task<List<MedicationSearchResult>> SearchMedicationsAsync(string searchTerm);
        Task<Medication?> GetMedicationByRxCuiAsync(string rxCui);
        Task<Medication?> GetMedicationByIdAsync(int id);

        // User medication operations (CRUD)
        Task<UserMedication> AddUserMedicationAsync(AddUserMedicationRequest request);
        Task<UserMedication> UpdateUserMedicationAsync(int id, UpdateUserMedicationRequest request);
        Task<bool> DeleteUserMedicationAsync(int id);
        Task<List<UserMedication>> GetActiveUserMedicationsAsync();
        Task<List<UserMedication>> GetAllUserMedicationsAsync();
        Task<UserMedication?> GetUserMedicationByIdAsync(int id);

        // Medication set operations (Save/Load)
        Task<MedicationSet> SaveMedicationSetAsync(string name, string description, List<UserMedication> medications);
        Task<List<MedicationSet>> GetSavedMedicationSetsAsync();
        Task<MedicationSet?> GetMedicationSetByIdAsync(int id);
        Task<List<UserMedication>> LoadMedicationSetAsync(int setId);
        Task<bool> DeleteMedicationSetAsync(int id);
    }
}