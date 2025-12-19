using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormCandidate
    {
        [JsonPropertyName("rxcui")]
        public string RxCui { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public string ScoreString { get; set; } = "0";

        [JsonIgnore]
        public int Score => int.TryParse(ScoreString, out var score) ? score : 0;

        [JsonPropertyName("rank")]
        public string? Rank { get; set; }
    }
}
