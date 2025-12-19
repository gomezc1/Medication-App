using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Schedule;

namespace MedicationManager.Core.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<DailySchedule> GenerateScheduleAsync(List<UserMedication> medications);
        Task<byte[]> ExportScheduleToPdfAsync(DailySchedule schedule);
    }
}