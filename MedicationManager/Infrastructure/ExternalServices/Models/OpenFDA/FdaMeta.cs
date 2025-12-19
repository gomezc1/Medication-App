using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA
{
    public class FdaMeta
    {
        [JsonPropertyName("disclaimer")]
        public string? Disclaimer { get; set; }

        [JsonPropertyName("terms")]
        public string? Terms { get; set; }

        [JsonPropertyName("license")]
        public string? License { get; set; }

        [JsonPropertyName("last_updated")]
        public string? LastUpdated { get; set; }

        [JsonPropertyName("results")]
        public FdaMetaResults? Results { get; set; }
    }
}