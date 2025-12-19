using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormPropertiesResponse
    {
        [JsonPropertyName("properties")]
        public RxNormProperties? Properties { get; set; }
    }
}
