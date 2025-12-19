using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MedicationManager.Infrastructure.ExternalServices
{
    public static class ApiServiceExtensionsWithCache
    {
        /// <summary>
        /// Add API services with memory caching enabled
        /// </summary>
        public static IServiceCollection AddExternalApiServicesWithCache(this IServiceCollection services)
        {
            // Add memory cache
            services.AddMemoryCache();

            // Add base API services (without caching)
            services.AddExternalApiServices();

            // Decorate with caching layer
            services.Decorate<IRxNormApiService, CachedRxNormApiService>();
            services.Decorate<IOpenFdaApiService, CachedOpenFdaApiService>();

            return services;
        }
    }
}