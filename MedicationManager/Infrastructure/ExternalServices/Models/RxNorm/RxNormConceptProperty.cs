using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.RxNorm
{
    public class RxNormConceptProperty
    {
        [JsonPropertyName("rxcui")]
        public string RxCui { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("synonym")]
        public string? Synonym { get; set; }

        [JsonPropertyName("tty")]
        public string? Tty { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("suppress")]
        public string? Suppress { get; set; }

        [JsonPropertyName("umlscui")]
        public string? UmlsCui { get; set; }
    }
}
