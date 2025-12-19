using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormApproximateGroup
    {
        [JsonPropertyName("candidate")]
        public List<RxNormCandidate>? Candidates { get; set; }
    }
}
