using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA
{
    public class FdaDrugResponse
    {
        [JsonPropertyName("meta")]
        public FdaMeta? Meta { get; set; }

        [JsonPropertyName("results")]
        public List<FdaDrugResult>? Results { get; set; }
    }
}