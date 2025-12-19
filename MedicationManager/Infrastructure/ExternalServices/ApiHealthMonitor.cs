using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MedicationManager.Infrastructure.ExternalServices
{
    /// <summary>
    /// Monitors the health of external API services
    /// </summary>
    public class ApiHealthMonitor
    {
        private readonly IOpenFdaApiService _fdaService;
        private readonly IRxNormApiService _rxNormService;
        private readonly ILogger<ApiHealthMonitor> _logger;

        private readonly Dictionary<string, ApiHealthStatus> _healthStatuses = new();

        public ApiHealthMonitor(
            IOpenFdaApiService fdaService,
            IRxNormApiService rxNormService,
            ILogger<ApiHealthMonitor> logger)
        {
            _fdaService = fdaService;
            _rxNormService = rxNormService;
            _logger = logger;

            InitializeHealthStatuses();
        }

        private void InitializeHealthStatuses()
        {
            _healthStatuses["OpenFDA"] = new ApiHealthStatus
            {
                ApiName = "OpenFDA",
                IsHealthy = true,
                LastChecked = DateTime.MinValue
            };

            _healthStatuses["RxNorm"] = new ApiHealthStatus
            {
                ApiName = "RxNorm",
                IsHealthy = true,
                LastChecked = DateTime.MinValue
            };
        }

        public async Task<ApiHealthStatus> CheckOpenFdaHealthAsync()
        {
            var status = _healthStatuses["OpenFDA"];
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Perform a simple test query
                var result = await _fdaService.SearchDrugsAsync("aspirin", 1);

                stopwatch.Stop();

                status.IsHealthy = true;
                status.LastChecked = DateTime.Now;
                status.ResponseTime = stopwatch.Elapsed;
                status.ErrorMessage = null;
                status.ConsecutiveFailures = 0;

                _logger.LogInformation("OpenFDA API health check passed in {Ms}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                status.IsHealthy = false;
                status.LastChecked = DateTime.Now;
                status.ResponseTime = stopwatch.Elapsed;
                status.ErrorMessage = ex.Message;
                status.ConsecutiveFailures++;

                _logger.LogError(ex, "OpenFDA API health check failed");
            }

            return status;
        }

        public async Task<ApiHealthStatus> CheckRxNormHealthAsync()
        {
            var status = _healthStatuses["RxNorm"];
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Perform a simple test query
                var result = await _rxNormService.SearchApproximateMatchAsync("aspirin");

                stopwatch.Stop();

                status.IsHealthy = true;
                status.LastChecked = DateTime.Now;
                status.ResponseTime = stopwatch.Elapsed;
                status.ErrorMessage = null;
                status.ConsecutiveFailures = 0;

                _logger.LogInformation("RxNorm API health check passed in {Ms}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                status.IsHealthy = false;
                status.LastChecked = DateTime.Now;
                status.ResponseTime = stopwatch.Elapsed;
                status.ErrorMessage = ex.Message;
                status.ConsecutiveFailures++;

                _logger.LogError(ex, "RxNorm API health check failed");
            }

            return status;
        }

        public async Task<Dictionary<string, ApiHealthStatus>> CheckAllApisAsync()
        {
            await Task.WhenAll(
                CheckOpenFdaHealthAsync(),
                CheckRxNormHealthAsync()
            );

            return new Dictionary<string, ApiHealthStatus>(_healthStatuses);
        }

        public ApiHealthStatus GetHealthStatus(string apiName)
        {
            return _healthStatuses.TryGetValue(apiName, out var status)
                ? status
                : new ApiHealthStatus { ApiName = apiName, IsHealthy = false };
        }

        public bool AreAllApisHealthy()
        {
            foreach (var status in _healthStatuses.Values)
            {
                if (!status.IsHealthy)
                    return false;
            }
            return true;
        }
    }
}