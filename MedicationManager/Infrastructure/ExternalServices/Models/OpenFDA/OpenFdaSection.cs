using System.Text.Json.Serialization;

namespace MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA
{
    public class OpenFdaSection
    {
        [JsonPropertyName("brand_name")]
        public List<string>? BrandName { get; set; }

        [JsonPropertyName("generic_name")]
        public List<string>? GenericName { get; set; }

        [JsonPropertyName("manufacturer_name")]
        public List<string>? ManufacturerName { get; set; }

        [JsonPropertyName("product_type")]
        public List<string>? ProductType { get; set; }

        [JsonPropertyName("route")]
        public List<string>? Route { get; set; }

        [JsonPropertyName("substance_name")]
        public List<string>? SubstanceName { get; set; }

        [JsonPropertyName("rxcui")]
        public List<string>? RxCui { get; set; }

        [JsonPropertyName("spl_id")]
        public List<string>? SplId { get; set; }

        [JsonPropertyName("package_ndc")]
        public List<string>? PackageNdc { get; set; }

        [JsonPropertyName("product_ndc")]
        public List<string>? ProductNdc { get; set; }
    }
}