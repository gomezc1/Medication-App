using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormConceptGroup
    {
        [JsonPropertyName("tty")]
        public string? Tty { get; set; }

        [JsonPropertyName("conceptProperties")]
        public List<RxNormConceptProperty>? ConceptProperties { get; set; }
    }
}
