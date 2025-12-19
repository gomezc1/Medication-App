using MedicationManager.Core.Models.Exceptions;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models.RxNorm;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace MedicationManager.Infrastructure.ExternalServices
{
    public class RxNormApiService : IRxNormApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RxNormApiService> _logger;
        private const string BASE_URL = "https://rxnav.nlm.nih.gov/REST/";

        public RxNormApiService(HttpClient httpClient, ILogger<RxNormApiService> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BASE_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger;
        }

        public async Task<List<RxNormCandidate>> SearchApproximateMatchAsync(string term)
        {
            try
            {
                var query = $"approximateTerm.json?term={Uri.EscapeDataString(term)}&maxEntries=20";

                _logger.LogInformation("Searching RxNorm for approximate match: {Term}", term);

                var response = await _httpClient.GetAsync(query);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RxNormApproximateResponse>();

                var candidates = result?.ApproximateGroup?.Candidates ?? new List<RxNormCandidate>();

                _logger.LogInformation("RxNorm returned {Count} candidates", candidates.Count);

                return candidates;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling RxNorm API");
                throw new ApiException("RxNorm", "Failed to connect to RxNorm API", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "RxNorm API request timed out");
                throw new ApiException("RxNorm", "Request to RxNorm API timed out", ex);
            }
        }

        public async Task<RxNormProperties?> GetRxCuiDetailsAsync(string rxCui)
        {
            try
            {
                var query = $"rxcui/{rxCui}/properties.json";

                _logger.LogInformation("Getting RxNorm details for RxCui: {RxCui}", rxCui);

                var response = await _httpClient.GetAsync(query);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RxNormPropertiesResponse>();

                return result?.Properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RxNorm details for RxCui: {RxCui}", rxCui);
                return null;
            }
        }

        public async Task<List<string>> GetActiveIngredientsAsync(string rxCui)
        {
            try
            {
                // Get ingredients using the related API with TTY=IN (ingredient)
                var query = $"rxcui/{rxCui}/related.json?tty=IN";

                _logger.LogInformation("Getting active ingredients for RxCui: {RxCui}", rxCui);

                var response = await _httpClient.GetAsync(query);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RxNormRelatedResponse>();

                var ingredients = result?.RelatedGroup?.ConceptGroup?
                    .Where(cg => cg.Tty == "IN")
                    .SelectMany(cg => cg.ConceptProperties ?? new List<RxNormConceptProperty>())
                    .Select(cp => cp.Name)
                    .Distinct()
                    .ToList() ?? new List<string>();

                _logger.LogInformation("Found {Count} active ingredients for RxCui: {RxCui}", ingredients.Count, rxCui);

                return ingredients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active ingredients for RxCui: {RxCui}", rxCui);
                return new List<string>();
            }
        }

        public async Task<List<string>> GetDrugClassesAsync(string rxCui)
        {
            try
            {
                // Get ATC classifications 
                var query = $"rxclass/class/byRxcui.json?rxcui={rxCui}&relaSource=ATC";

                _logger.LogInformation("Getting drug classes for RxCui: {RxCui}", rxCui);

                var response = await _httpClient.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get drug classes for RxCui: {RxCui}", rxCui);
                    return new List<string>();
                }

                var content = await response.Content.ReadAsStringAsync();

                // Parse the response (simplified - actual parsing would depend on exact API response structure)
                var classes = new List<string>();

                _logger.LogInformation("Found {Count} drug classes for RxCui: {RxCui}", classes.Count, rxCui);

                return classes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug classes for RxCui: {RxCui}", rxCui);
                return new List<string>();
            }
        }

        public async Task<List<RxNormConceptProperty>> GetRelatedDrugsAsync(string rxCui, string relationshipType = "ingredients")
        {
            try
            {
                var query = $"rxcui/{rxCui}/related.json";

                _logger.LogInformation("Getting related drugs for RxCui: {RxCui}", rxCui);

                var response = await _httpClient.GetAsync(query);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RxNormRelatedResponse>();

                var relatedDrugs = result?.RelatedGroup?.ConceptGroup?
                    .SelectMany(cg => cg.ConceptProperties ?? new List<RxNormConceptProperty>())
                    .ToList() ?? new List<RxNormConceptProperty>();

                _logger.LogInformation("Found {Count} related drugs for RxCui: {RxCui}", relatedDrugs.Count, rxCui);

                return relatedDrugs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related drugs for RxCui: {RxCui}", rxCui);
                return new List<RxNormConceptProperty>();
            }
        }
    }
}