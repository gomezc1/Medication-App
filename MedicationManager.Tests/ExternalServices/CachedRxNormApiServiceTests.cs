using FluentAssertions;
using MedicationManager.Infrastructure.ExternalServices;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Models.RxNorm;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedicationManager.Tests.ExternalServices
{
    public class CachedRxNormApiServiceTests
    {
        private readonly Mock<IRxNormApiService> _mockInnerService;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<CachedRxNormApiService>> _mockLogger;
        private readonly CachedRxNormApiService _service;

        public CachedRxNormApiServiceTests()
        {
            _mockInnerService = new Mock<IRxNormApiService>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<CachedRxNormApiService>>();
            _service = new CachedRxNormApiService(_mockInnerService.Object, _cache, _mockLogger.Object);
        }

        [Fact]
        public async Task SearchApproximateMatchAsync_FirstCall_CallsInnerServiceAndCaches()
        {
            // Arrange
            var searchTerm = "tylenol";
            var expectedResult = new List<RxNormCandidate>
            {
                new RxNormCandidate { RxCui = "161", Name = "Tylenol", ScoreString = "100" }
            };

            _mockInnerService
                .Setup(s => s.SearchApproximateMatchAsync(searchTerm))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SearchApproximateMatchAsync(searchTerm);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _mockInnerService.Verify(s => s.SearchApproximateMatchAsync(searchTerm), Times.Once);
        }

        [Fact]
        public async Task SearchApproximateMatchAsync_SecondCall_ReturnsCachedResult()
        {
            // Arrange
            var searchTerm = "tylenol";
            var expectedResult = new List<RxNormCandidate>
            {
                new RxNormCandidate { RxCui = "161", Name = "Tylenol", ScoreString = "100" }
            };

            _mockInnerService
                .Setup(s => s.SearchApproximateMatchAsync(searchTerm))
                .ReturnsAsync(expectedResult);

            // Act
            var result1 = await _service.SearchApproximateMatchAsync(searchTerm);
            var result2 = await _service.SearchApproximateMatchAsync(searchTerm);

            // Assert
            result1.Should().BeEquivalentTo(expectedResult);
            result2.Should().BeEquivalentTo(expectedResult);
            _mockInnerService.Verify(s => s.SearchApproximateMatchAsync(searchTerm), Times.Once);
        }

        [Fact]
        public async Task GetRxCuiDetailsAsync_WithCache_ReturnsCachedResult()
        {
            // Arrange
            var rxCui = "161";
            var expectedResult = new RxNormProperties
            {
                RxCui = rxCui,
                Name = "Acetaminophen"
            };

            _mockInnerService
                .Setup(s => s.GetRxCuiDetailsAsync(rxCui))
                .ReturnsAsync(expectedResult);

            // Act
            var result1 = await _service.GetRxCuiDetailsAsync(rxCui);
            var result2 = await _service.GetRxCuiDetailsAsync(rxCui);

            // Assert
            result1.Should().BeEquivalentTo(expectedResult);
            result2.Should().BeEquivalentTo(expectedResult);
            _mockInnerService.Verify(s => s.GetRxCuiDetailsAsync(rxCui), Times.Once);
        }

        [Fact]
        public async Task GetActiveIngredientsAsync_WithCache_ReturnsCachedResult()
        {
            // Arrange
            var rxCui = "161";
            var expectedResult = new List<string> { "Acetaminophen" };

            _mockInnerService
                .Setup(s => s.GetActiveIngredientsAsync(rxCui))
                .ReturnsAsync(expectedResult);

            // Act
            var result1 = await _service.GetActiveIngredientsAsync(rxCui);
            var result2 = await _service.GetActiveIngredientsAsync(rxCui);

            // Assert
            result1.Should().BeEquivalentTo(expectedResult);
            result2.Should().BeEquivalentTo(expectedResult);
            _mockInnerService.Verify(s => s.GetActiveIngredientsAsync(rxCui), Times.Once);
        }

        [Fact]
        public async Task GetRxCuiDetailsAsync_WithNullResult_DoesNotCache()
        {
            // Arrange
            var rxCui = "99999999";

            _mockInnerService
                .Setup(s => s.GetRxCuiDetailsAsync(rxCui))
                .ReturnsAsync((RxNormProperties?)null);

            // Act
            var result1 = await _service.GetRxCuiDetailsAsync(rxCui);
            var result2 = await _service.GetRxCuiDetailsAsync(rxCui);

            // Assert
            result1.Should().BeNull();
            result2.Should().BeNull();
            _mockInnerService.Verify(s => s.GetRxCuiDetailsAsync(rxCui), Times.Exactly(2));
        }
    }
}