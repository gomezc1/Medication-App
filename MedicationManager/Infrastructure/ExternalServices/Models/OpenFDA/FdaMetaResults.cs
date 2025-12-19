using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA
{
    public class FdaMetaResults
    {
        [JsonPropertyName("skip")]
        public int Skip { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}