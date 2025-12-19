using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA
{
    public class FdaDrugResult
    {
        [JsonPropertyName("openfda")]
        public OpenFdaSection? OpenFda { get; set; }

        [JsonPropertyName("drug_interactions")]
        public List<string>? DrugInteractions { get; set; }

        [JsonPropertyName("warnings")]
        public List<string>? Warnings { get; set; }

        [JsonPropertyName("dosage_and_administration")]
        public List<string>? DosageAndAdministration { get; set; }

        [JsonPropertyName("indications_and_usage")]
        public List<string>? IndicationsAndUsage { get; set; }

        [JsonPropertyName("adverse_reactions")]
        public List<string>? AdverseReactions { get; set; }
    }
}