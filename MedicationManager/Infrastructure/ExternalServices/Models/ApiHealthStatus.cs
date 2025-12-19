namespace MedicationManager.Infrastructure.ExternalServices.Models
{
    /// <summary>
    /// Health status for external API services
    /// </summary>
    public class ApiHealthStatus
    {
        public string ApiName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public DateTime LastChecked { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string? ErrorMessage { get; set; }
        public int ConsecutiveFailures { get; set; }
    }
}