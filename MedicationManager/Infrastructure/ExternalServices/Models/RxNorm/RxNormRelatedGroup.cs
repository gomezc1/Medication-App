using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormRelatedGroup
    {
        [JsonPropertyName("conceptGroup")]
        public List<RxNormConceptGroup>? ConceptGroup { get; set; }
    }
}
