namespace MedicationManager.Core.Services.Interfaces
{
    public interface IDataSeedService
    {
        Task SeedInitialDataAsync();
        Task SeedOtcMedicationsAsync();
        Task SeedInteractionDataAsync();
    }
}