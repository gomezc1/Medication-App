using FluentAssertions;
using MedicationManager.Infrastructure.ExternalServices;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA;
using MedicationManager.Infrastructure.ExternalServices.Models.RxNorm;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedicationManager.Tests.ExternalServices
{
    public class ApiHealthMonitorTests
    {
        private readonly Mock<IOpenFdaApiService> _mockFdaService;
        private readonly Mock<IRxNormApiService> _mockRxNormService;
        private readonly Mock<ILogger<ApiHealthMonitor>> _mockLogger;
        private readonly ApiHealthMonitor _monitor;

        public ApiHealthMonitorTests()
        {
            _mockFdaService = new Mock<IOpenFdaApiService>();
            _mockRxNormService = new Mock<IRxNormApiService>();
            _mockLogger = new Mock<ILogger<ApiHealthMonitor>>();
            _monitor = new ApiHealthMonitor(_mockFdaService.Object, _mockRxNormService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CheckOpenFdaHealthAsync_WhenHealthy_ReturnsHealthyStatus()
        {
            // Arrange
            _mockFdaService
                .Setup(s => s.SearchDrugsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new FdaDrugResponse());

            // Act
            var result = await _monitor.CheckOpenFdaHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeTrue();
            result.ApiName.Should().Be("OpenFDA");
            result.ConsecutiveFailures.Should().Be(0);
            result.ResponseTime.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CheckOpenFdaHealthAsync_WhenUnhealthy_ReturnsUnhealthyStatus()
        {
            // Arrange
            _mockFdaService
                .Setup(s => s.SearchDrugsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("API Error"));

            // Act
            var result = await _monitor.CheckOpenFdaHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeFalse();
            result.ApiName.Should().Be("OpenFDA");
            result.ConsecutiveFailures.Should().Be(1);
            result.ErrorMessage.Should().Be("API Error");
        }

        [Fact]
        public async Task CheckRxNormHealthAsync_WhenHealthy_ReturnsHealthyStatus()
        {
            // Arrange
            _mockRxNormService
                .Setup(s => s.SearchApproximateMatchAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Collections.Generic.List<RxNormCandidate>());

            // Act
            var result = await _monitor.CheckRxNormHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeTrue();
            result.ApiName.Should().Be("RxNorm");
            result.ConsecutiveFailures.Should().Be(0);
        }

        [Fact]
        public async Task CheckRxNormHealthAsync_WhenUnhealthy_ReturnsUnhealthyStatus()
        {
            // Arrange
            _mockRxNormService
                .Setup(s => s.SearchApproximateMatchAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Network Error"));

            // Act
            var result = await _monitor.CheckRxNormHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsHealthy.Should().BeFalse();
            result.ConsecutiveFailures.Should().Be(1);
            result.ErrorMessage.Should().Be("Network Error");
        }

        [Fact]
        public async Task CheckAllApisAsync_ReturnsAllStatuses()
        {
            // Arrange
            _mockFdaService
                .Setup(s => s.SearchDrugsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new FdaDrugResponse());

            _mockRxNormService
                .Setup(s => s.SearchApproximateMatchAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Collections.Generic.List<RxNormCandidate>());

            // Act
            var result = await _monitor.CheckAllApisAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainKey("OpenFDA");
            result.Should().ContainKey("RxNorm");
        }

        [Fact]
        public void AreAllApisHealthy_WhenAllHealthy_ReturnsTrue()
        {
            // Arrange
            _mockFdaService
                .Setup(s => s.SearchDrugsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new FdaDrugResponse());

            _mockRxNormService
                .Setup(s => s.SearchApproximateMatchAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Collections.Generic.List<RxNormCandidate>());

            // Act - need to check first
            _ = _monitor.CheckAllApisAsync().Result;
            var result = _monitor.AreAllApisHealthy();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetHealthStatus_WithValidApiName_ReturnsStatus()
        {
            // Act
            var result = _monitor.GetHealthStatus("OpenFDA");

            // Assert
            result.Should().NotBeNull();
            result.ApiName.Should().Be("OpenFDA");
        }

        [Fact]
        public void GetHealthStatus_WithInvalidApiName_ReturnsUnhealthyStatus()
        {
            // Act
            var result = _monitor.GetHealthStatus("InvalidAPI");

            // Assert
            result.Should().NotBeNull();
            result.ApiName.Should().Be("InvalidAPI");
            result.IsHealthy.Should().BeFalse();
        }
    }
}