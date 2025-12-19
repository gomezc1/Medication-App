using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormRelatedResponse
    {
        [JsonPropertyName("relatedGroup")]
        public RxNormRelatedGroup? RelatedGroup { get; set; }
    }
}
