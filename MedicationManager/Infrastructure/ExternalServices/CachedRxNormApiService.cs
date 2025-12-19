using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models.RxNorm;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Infrastructure.ExternalServices
{
    /// <summary>
    /// Caching wrapper for API services to reduce external calls
    /// </summary>
    public class CachedRxNormApiService : IRxNormApiService
    {
        private readonly IRxNormApiService _innerService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedRxNormApiService> _logger;

        // Cache durations
        private readonly TimeSpan _searchCacheDuration = TimeSpan.FromHours(24);
        private readonly TimeSpan _detailsCacheDuration = TimeSpan.FromDays(7);
        private readonly TimeSpan _ingredientsCacheDuration = TimeSpan.FromDays(30);

        public CachedRxNormApiService(
            IRxNormApiService innerService,
            IMemoryCache cache,
            ILogger<CachedRxNormApiService> logger)
        {
            _innerService = innerService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<RxNormCandidate>> SearchApproximateMatchAsync(string term)
        {
            var cacheKey = $"rxnorm_search_{term.ToLowerInvariant()}";

            if (_cache.TryGetValue(cacheKey, out List<RxNormCandidate>? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for RxNorm search: {Term}", term);
                return cached;
            }

            _logger.LogDebug("Cache miss for RxNorm search: {Term}", term);
            var result = await _innerService.SearchApproximateMatchAsync(term);

            _cache.Set(cacheKey, result, _searchCacheDuration);

            return result;
        }

        public async Task<RxNormProperties?> GetRxCuiDetailsAsync(string rxCui)
        {
            var cacheKey = $"rxnorm_details_{rxCui}";

            if (_cache.TryGetValue(cacheKey, out RxNormProperties? cached))
            {
                _logger.LogDebug("Cache hit for RxCui details: {RxCui}", rxCui);
                return cached;
            }

            _logger.LogDebug("Cache miss for RxCui details: {RxCui}", rxCui);
            var result = await _innerService.GetRxCuiDetailsAsync(rxCui);

            if (result != null)
            {
                _cache.Set(cacheKey, result, _detailsCacheDuration);
            }

            return result;
        }

        public async Task<List<string>> GetActiveIngredientsAsync(string rxCui)
        {
            var cacheKey = $"rxnorm_ingredients_{rxCui}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for active ingredients: {RxCui}", rxCui);
                return cached;
            }

            _logger.LogDebug("Cache miss for active ingredients: {RxCui}", rxCui);
            var result = await _innerService.GetActiveIngredientsAsync(rxCui);

            _cache.Set(cacheKey, result, _ingredientsCacheDuration);

            return result;
        }

        public async Task<List<string>> GetDrugClassesAsync(string rxCui)
        {
            var cacheKey = $"rxnorm_classes_{rxCui}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for drug classes: {RxCui}", rxCui);
                return cached;
            }

            _logger.LogDebug("Cache miss for drug classes: {RxCui}", rxCui);
            var result = await _innerService.GetDrugClassesAsync(rxCui);

            _cache.Set(cacheKey, result, _detailsCacheDuration);

            return result;
        }

        public async Task<List<RxNormConceptProperty>> GetRelatedDrugsAsync(string rxCui, string relationshipType = "ingredients")
        {
            var cacheKey = $"rxnorm_related_{rxCui}_{relationshipType}";

            if (_cache.TryGetValue(cacheKey, out List<RxNormConceptProperty>? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit for related drugs: {RxCui}", rxCui);
                return cached;
            }

            _logger.LogDebug("Cache miss for related drugs: {RxCui}", rxCui);
            var result = await _innerService.GetRelatedDrugsAsync(rxCui, relationshipType);

            _cache.Set(cacheKey, result, _detailsCacheDuration);

            return result;
        }
    }
}