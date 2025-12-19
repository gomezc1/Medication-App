using MedicationManager.Core.Models;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Infrastructure.ExternalServices
{
    /// <summary>
    /// Caching wrapper for OpenFDA API service
    /// </summary>
    public class CachedOpenFdaApiService : IOpenFdaApiService
    {
        private readonly IOpenFdaApiService _innerService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedOpenFdaApiService> _logger;

        private readonly TimeSpan _searchCacheDuration = TimeSpan.FromHours(24);
        private readonly TimeSpan _labelCacheDuration = TimeSpan.FromDays(7);
        private readonly TimeSpan _interactionCacheDuration = TimeSpan.FromDays(30);

        public CachedOpenFdaApiService(
            IOpenFdaApiService innerService,
            IMemoryCache cache,
            ILogger<CachedOpenFdaApiService> logger)
        {
            _innerService = innerService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<FdaDrugResponse> SearchDrugsAsync(string searchTerm, int limit = 10)
        {
            var cacheKey = $"openfda_search_{searchTerm.ToLowerInvariant()}_{limit}";

            if (_cache.TryGetValue(cacheKey, out FdaDrugResponse? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for OpenFDA search: {SearchTerm}", searchTerm);
                return cached;
            }

            _logger.LogDebug("Cache miss for OpenFDA search: {SearchTerm}", searchTerm);
            var result = await _innerService.SearchDrugsAsync(searchTerm, limit);

            _cache.Set(cacheKey, result, _searchCacheDuration);

            return result;
        }

        public async Task<FdaDrugResponse> SearchByRxCuiAsync(string rxCui)
        {
            var cacheKey = $"openfda_rxcui_{rxCui}";

            if (_cache.TryGetValue(cacheKey, out FdaDrugResponse? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for OpenFDA RxCui search: {RxCui}", rxCui);
                return cached;
            }

            _logger.LogDebug("Cache miss for OpenFDA RxCui search: {RxCui}", rxCui);
            var result = await _innerService.SearchByRxCuiAsync(rxCui);

            _cache.Set(cacheKey, result, _labelCacheDuration);

            return result;
        }

        public async Task<List<Core.Models.DrugInteraction>> GetDrugInteractionsAsync(string rxCui)
        {
            var cacheKey = $"openfda_interactions_{rxCui}";

            if (_cache.TryGetValue(cacheKey, out List<Core.Models.DrugInteraction>? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for drug interactions: {RxCui}", rxCui);
                return cached;
            }

            _logger.LogDebug("Cache miss for drug interactions: {RxCui}", rxCui);
            var result = await _innerService.GetDrugInteractionsAsync(rxCui);

            _cache.Set(cacheKey, result, _interactionCacheDuration);

            return result;
        }

        public async Task<FdaDrugResult?> GetDrugLabelAsync(string ndc)
        {
            var cacheKey = $"openfda_label_{ndc}";

            if (_cache.TryGetValue(cacheKey, out FdaDrugResult? cached))
            {
                _logger.LogDebug("Cache hit for drug label: {Ndc}", ndc);
                return cached;
            }

            _logger.LogDebug("Cache miss for drug label: {Ndc}", ndc);
            var result = await _innerService.GetDrugLabelAsync(ndc);

            if (result != null)
            {
                _cache.Set(cacheKey, result, _labelCacheDuration);
            }

            return result;
        }

        public async Task<List<DrugInteraction>> GetDrugInteractionsByNameAsync(string drugName1, string drugName2)
        {
            var cacheKey = $"openfda_interaction_{drugName1}_{drugName2}";

            if (_cache.TryGetValue(cacheKey, out List<DrugInteraction>? cached))
            {
                _logger.LogDebug("Cache hit for drug interaction: {DrugName1} and {DrugName2}", drugName1, drugName2);
                return cached;
            }

            _logger.LogDebug("Cache miss for drug label: {DrugName1} and {DrugName2}", drugName1, drugName2);
            var result = await _innerService.GetDrugInteractionsByNameAsync(drugName1, drugName2);

            if (result != null)
            {
                _cache.Set(cacheKey, result, _labelCacheDuration);
            }

            return result;
        }
    }
}