using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Exceptions;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace MedicationManager.Infrastructure.ExternalServices
{
    public class OpenFdaApiService : IOpenFdaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenFdaApiService> _logger;
        private const string BASE_URL = "https://api.fda.gov/drug/";

        // Rate limiting: 240 requests per minute, 120,000 per day
        private static readonly SemaphoreSlim RateLimiter = new(4, 4); // 4 requests per second
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public OpenFdaApiService(HttpClient httpClient, ILogger<OpenFdaApiService> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BASE_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger;
        }

        public async Task<FdaDrugResponse> SearchDrugsAsync(string searchTerm, int limit = 10)
        {
            try
            {
                await ApplyRateLimitAsync();

                // Clean and escape search term
                var cleanTerm = CleanSearchTerm(searchTerm);
                var query = $"label.json?search=openfda.brand_name:\"{cleanTerm}\"+openfda.generic_name:\"{cleanTerm}\"&limit={limit}";

                _logger.LogInformation("Searching OpenFDA for: {SearchTerm}", searchTerm);

                var response = await _httpClient.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenFDA search failed with status: {StatusCode}", response.StatusCode);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new FdaDrugResponse { Results = new List<FdaDrugResult>() };
                    }

                    throw new ApiException("OpenFDA", $"API returned status code: {response.StatusCode}", response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<FdaDrugResponse>();

                _logger.LogInformation("OpenFDA returned {Count} results", result?.Results?.Count ?? 0);

                return result ?? new FdaDrugResponse { Results = new List<FdaDrugResult>() };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling OpenFDA API");
                throw new ApiException("OpenFDA", "Failed to connect to OpenFDA API", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "OpenFDA API request timed out");
                throw new ApiException("OpenFDA", "Request to OpenFDA API timed out", ex);
            }
        }

        public async Task<FdaDrugResponse> SearchByRxCuiAsync(string rxCui)
        {
            try
            {
                await ApplyRateLimitAsync();

                var query = $"label.json?search=openfda.rxcui:\"{rxCui}\"&limit=1";

                _logger.LogInformation("Searching OpenFDA by RxCui: {RxCui}", rxCui);

                var response = await _httpClient.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new FdaDrugResponse { Results = new List<FdaDrugResult>() };
                    }

                    throw new ApiException("OpenFDA", $"API returned status code: {response.StatusCode}", response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<FdaDrugResponse>();
                return result ?? new FdaDrugResponse { Results = new List<FdaDrugResult>() };
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error searching OpenFDA by RxCui: {RxCui}", rxCui);
                throw new ApiException("OpenFDA", "Failed to search by RxCui", ex);
            }
        }

        public async Task<List<DrugInteraction>> GetDrugInteractionsAsync(string rxCui)
        {
            try
            {
                var fdaResponse = await SearchByRxCuiAsync(rxCui);
                var interactions = new List<DrugInteraction>();

                if (fdaResponse.Results == null || !fdaResponse.Results.Any())
                {
                    return interactions;
                }

                var drugResult = fdaResponse.Results.First();

                if (drugResult.DrugInteractions != null && drugResult.DrugInteractions.Any())
                {
                    foreach (var interactionText in drugResult.DrugInteractions)
                    {
                        var interaction = ParseInteractionText(interactionText, rxCui);
                        if (interaction != null)
                        {
                            interactions.Add(interaction);
                        }
                    }
                }

                _logger.LogInformation("Found {Count} interactions for RxCui: {RxCui}", interactions.Count, rxCui);

                return interactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug interactions for RxCui: {RxCui}", rxCui);
                return new List<DrugInteraction>();
            }
        }

        public async Task<List<DrugInteraction>> GetDrugInteractionsByNameAsync(string drugName1, string drugName2)
        {
            try
            {
                var interactions = new List<DrugInteraction>();

                if (string.IsNullOrWhiteSpace(drugName1) || string.IsNullOrWhiteSpace(drugName2))
                {
                    return interactions;
                }

                var clean1 = CleanSearchTerm(drugName1);
                var clean2 = CleanSearchTerm(drugName2);

                // Build query using the example pattern:
                // label.json?search=drug_interactions:aspirin+AND+drug_interactions:ibuprofen
                var encoded1 = Uri.EscapeDataString(clean1);
                var encoded2 = Uri.EscapeDataString(clean2);
                var query = $"label.json?search=drug_interactions:{encoded1}+AND+drug_interactions:{encoded2}&limit=100";

                _logger.LogInformation("Searching OpenFDA for interactions between {Drug1} and {Drug2}", drugName1, drugName2);

                await ApplyRateLimitAsync();

                var response = await _httpClient.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenFDA interaction search failed with status: {StatusCode}", response.StatusCode);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return interactions;
                    }

                    throw new ApiException("OpenFDA", $"API returned status code: {response.StatusCode}", response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<FdaDrugResponse>();

                if (result?.Results == null || !result.Results.Any())
                {
                    return interactions;
                }

                var seen = new HashSet<string>();

                foreach (var drugResult in result.Results)
                {
                    if (drugResult.DrugInteractions == null)
                    {
                        continue;
                    }

                    foreach (var interactionText in drugResult.DrugInteractions)
                    {
                        if (string.IsNullOrWhiteSpace(interactionText))
                        {
                            continue;
                        }

                        // Avoid duplicates
                        if (!seen.Add(interactionText))
                        {
                            continue;
                        }

                        // Use ParseInteractionText to determine severity and populate common fields.
                        // Pass drugName1 as the "rxCui" parameter so callers can trace origin;
                        // then set Drug2RxCui to the second drug name.
                        var parsed = ParseInteractionText(interactionText, drugName1);
                        if (parsed != null)
                        {
                            parsed.Drug2RxCui = drugName2;
                            interactions.Add(parsed);
                        }
                    }
                }

                _logger.LogInformation("Found {Count} aggregated interactions for {Drug1} and {Drug2}", interactions.Count, drugName1, drugName2);

                return interactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug interactions for drugs: {Drug1} and {Drug2}", drugName1, drugName2);
                return new List<DrugInteraction>();
            }
        }


        public async Task<FdaDrugResult?> GetDrugLabelAsync(string ndc)
        {
            try
            {
                await ApplyRateLimitAsync();

                var query = $"label.json?search=openfda.product_ndc:\"{ndc}\"&limit=1";

                _logger.LogInformation("Getting drug label for NDC: {Ndc}", ndc);

                var response = await _httpClient.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<FdaDrugResponse>();
                return result?.Results?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug label for NDC: {Ndc}", ndc);
                return null;
            }
        }

        private async Task ApplyRateLimitAsync()
        {
            await RateLimiter.WaitAsync();

            // Ensure minimum 250ms between requests (4 per second)
            var timeSinceLastRequest = DateTime.Now - _lastRequestTime;
            if (timeSinceLastRequest.TotalMilliseconds < 250)
            {
                await Task.Delay(250 - (int)timeSinceLastRequest.TotalMilliseconds);
            }

            _lastRequestTime = DateTime.Now;

            // Release after delay
            _ = Task.Delay(250).ContinueWith(_ => RateLimiter.Release());
        }

        private string CleanSearchTerm(string term)
        {
            // Remove special characters that might break the query
            term = Regex.Replace(term, @"[^\w\s-]", "");
            term = term.Trim();
            return term;
        }

        private DrugInteraction? ParseInteractionText(string interactionText, string rxCui)
        {
            if (string.IsNullOrWhiteSpace(interactionText))
            {
                return null;
            }

            // Determine severity based on keywords in the text
            var severity = DetermineSeverityFromText(interactionText);

            return new DrugInteraction
            {
                Drug1RxCui = rxCui,
                Drug2RxCui = "", // Will be filled in by InteractionService
                SeverityLevel = severity,
                InteractionType = "Drug-Drug Interaction",
                Description = interactionText.Length > 1000 ? interactionText.Substring(0, 1000) : interactionText,
                Source = "OpenFDA",
                SourceDate = DateTime.Now
            };
        }

        private InteractionSeverity DetermineSeverityFromText(string text)
        {
            var lowerText = text.ToLowerInvariant();

            // Contraindicated keywords
            if (lowerText.Contains("contraindicated") ||
                lowerText.Contains("do not use") ||
                lowerText.Contains("should not be used"))
            {
                return InteractionSeverity.Contraindicated;
            }

            // Major severity keywords
            if (lowerText.Contains("serious") ||
                lowerText.Contains("severe") ||
                lowerText.Contains("fatal") ||
                lowerText.Contains("life-threatening") ||
                lowerText.Contains("hospitalization"))
            {
                return InteractionSeverity.Major;
            }

            // Moderate severity keywords
            if (lowerText.Contains("caution") ||
                lowerText.Contains("monitor") ||
                lowerText.Contains("may increase") ||
                lowerText.Contains("may decrease"))
            {
                return InteractionSeverity.Moderate;
            }

            // Default to minor
            return InteractionSeverity.Minor;
        }
    }
}