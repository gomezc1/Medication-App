using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormApproximateResponse
    {
        [JsonPropertyName("approximateGroup")]
        public RxNormApproximateGroup? ApproximateGroup { get; set; }
    }
}
