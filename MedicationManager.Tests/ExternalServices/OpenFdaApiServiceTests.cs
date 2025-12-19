using FluentAssertions;
using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Exceptions;
using MedicationManager.Infrastructure.ExternalServices;
using MedicationManager.Infrastructure.ExternalServices.Models.OpenFDA;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace MedicationManager.Tests.ExternalServices
{
    public class OpenFdaApiServiceTests
    {
        private readonly Mock<ILogger<OpenFdaApiService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly OpenFdaApiService _service;

        public OpenFdaApiServiceTests()
        {
            _mockLogger = new Mock<ILogger<OpenFdaApiService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.fda.gov/drug/")
            };
            _service = new OpenFdaApiService(_httpClient, _mockLogger.Object);
        }

        #region SearchDrugsAsync Tests

        [Fact]
        public async Task SearchDrugsAsync_WithValidSearchTerm_ReturnsResults()
        {
            // Arrange
            var searchTerm = "tylenol";
            var expectedResponse = CreateMockFdaDrugResponse("Tylenol", "Acetaminophen");

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchDrugsAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().NotBeNull();
            result.Results.Should().HaveCount(1);
            result.Results.First().OpenFda?.BrandName?.First().Should().Be("Tylenol");
        }

        [Fact]
        public async Task SearchDrugsAsync_WithNotFoundResponse_ReturnsEmptyResults()
        {
            // Arrange
            var searchTerm = "nonexistentdrug12345";

            SetupHttpResponse(HttpStatusCode.NotFound, null);

            // Act
            var result = await _service.SearchDrugsAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().NotBeNull();
            result.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchDrugsAsync_WithSpecialCharacters_CleansSearchTerm()
        {
            // Arrange
            var searchTerm = "tylenol®";
            var expectedResponse = CreateMockFdaDrugResponse("Tylenol", "Acetaminophen");

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchDrugsAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SearchDrugsAsync_WithCustomLimit_UsesCorrectLimit()
        {
            // Arrange
            var searchTerm = "aspirin";
            var limit = 5;
            var expectedResponse = CreateMockFdaDrugResponseWithMultipleResults(limit);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchDrugsAsync(searchTerm, limit);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().HaveCount(limit);
        }

        [Fact]
        public async Task SearchDrugsAsync_WithHttpRequestException_ThrowsApiException()
        {
            // Arrange
            var searchTerm = "tylenol";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => _service.SearchDrugsAsync(searchTerm));
        }

        [Fact]
        public async Task SearchDrugsAsync_WithTimeout_ThrowsApiException()
        {
            // Arrange
            var searchTerm = "tylenol";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Request timed out"));

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => _service.SearchDrugsAsync(searchTerm));
        }

        #endregion

        #region SearchByRxCuiAsync Tests

        [Fact]
        public async Task SearchByRxCuiAsync_WithValidRxCui_ReturnsResult()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = CreateMockFdaDrugResponse("Tylenol", "Acetaminophen", rxCui);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchByRxCuiAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().NotBeEmpty();
            result.Results.First().OpenFda?.RxCui?.Should().Contain(rxCui);
        }

        [Fact]
        public async Task SearchByRxCuiAsync_WithInvalidRxCui_ReturnsEmptyResults()
        {
            // Arrange
            var rxCui = "99999999";

            SetupHttpResponse(HttpStatusCode.NotFound, null);

            // Act
            var result = await _service.SearchByRxCuiAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchByRxCuiAsync_WithServerError_ThrowsApiException()
        {
            // Arrange
            var rxCui = "161";

            SetupHttpResponse(HttpStatusCode.InternalServerError, null);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => _service.SearchByRxCuiAsync(rxCui));
        }

        #endregion

        #region GetDrugInteractionsAsync Tests

        [Fact]
        public async Task GetDrugInteractionsAsync_WithInteractions_ReturnsInteractionList()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = CreateMockFdaDrugResponseWithInteractions(rxCui);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetDrugInteractionsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.First().Drug1RxCui.Should().Be(rxCui);
            result.First().Source.Should().Be("OpenFDA");
        }

        [Fact]
        public async Task GetDrugInteractionsAsync_WithNoInteractions_ReturnsEmptyList()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = CreateMockFdaDrugResponse("Tylenol", "Acetaminophen", rxCui);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetDrugInteractionsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDrugInteractionsAsync_WithMajorInteraction_SetsSeverityCorrectly()
        {
            // Arrange
            var rxCui = "161";
            var interactionText = "This drug may cause serious or life-threatening reactions when combined with alcohol.";
            var expectedResponse = CreateMockFdaDrugResponseWithSpecificInteraction(rxCui, interactionText);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetDrugInteractionsAsync(rxCui);

            // Assert
            result.Should().NotBeEmpty();
            result.First().SeverityLevel.Should().Be(InteractionSeverity.Major);
        }

        [Fact]
        public async Task GetDrugInteractionsAsync_WithContraindicatedInteraction_SetsSeverityCorrectly()
        {
            // Arrange
            var rxCui = "161";
            var interactionText = "This medication is contraindicated with MAO inhibitors.";
            var expectedResponse = CreateMockFdaDrugResponseWithSpecificInteraction(rxCui, interactionText);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetDrugInteractionsAsync(rxCui);

            // Assert
            result.Should().NotBeEmpty();
            result.First().SeverityLevel.Should().Be(InteractionSeverity.Contraindicated);
        }

        [Fact]
        public async Task GetDrugInteractionsAsync_WithException_ReturnsEmptyList()
        {
            // Arrange
            var rxCui = "161";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _service.GetDrugInteractionsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetDrugLabelAsync Tests

        [Fact]
        public async Task GetDrugLabelAsync_WithValidNdc_ReturnsLabel()
        {
            // Arrange
            var ndc = "50580-506";
            var expectedResponse = CreateMockFdaDrugResponse("Tylenol", "Acetaminophen");

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetDrugLabelAsync(ndc);

            // Assert
            result.Should().NotBeNull();
            result.OpenFda.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDrugLabelAsync_WithInvalidNdc_ReturnsNull()
        {
            // Arrange
            var ndc = "invalid-ndc";

            SetupHttpResponse(HttpStatusCode.NotFound, null);

            // Act
            var result = await _service.GetDrugLabelAsync(ndc);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDrugLabelAsync_WithException_ReturnsNull()
        {
            // Arrange
            var ndc = "50580-506";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _service.GetDrugLabelAsync(ndc);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Helper Methods

        private void SetupHttpResponse(HttpStatusCode statusCode, FdaDrugResponse? response)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = response != null
                    ? new StringContent(JsonSerializer.Serialize(response))
                    : new StringContent("")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);
        }

        private FdaDrugResponse CreateMockFdaDrugResponse(string brandName, string genericName, string? rxCui = null)
        {
            return new FdaDrugResponse
            {
                Results = new List<FdaDrugResult>
                {
                    new FdaDrugResult
                    {
                        OpenFda = new OpenFdaSection
                        {
                            BrandName = new List<string> { brandName },
                            GenericName = new List<string> { genericName },
                            RxCui = rxCui != null ? new List<string> { rxCui } : new List<string>(),
                            ManufacturerName = new List<string> { "Test Manufacturer" },
                            ProductType = new List<string> { "HUMAN OTC DRUG" }
                        }
                    }
                }
            };
        }

        private FdaDrugResponse CreateMockFdaDrugResponseWithMultipleResults(int count)
        {
            var results = new List<FdaDrugResult>();
            for (int i = 0; i < count; i++)
            {
                results.Add(new FdaDrugResult
                {
                    OpenFda = new OpenFdaSection
                    {
                        BrandName = new List<string> { $"Brand {i}" },
                        GenericName = new List<string> { $"Generic {i}" }
                    }
                });
            }

            return new FdaDrugResponse { Results = results };
        }

        private FdaDrugResponse CreateMockFdaDrugResponseWithInteractions(string rxCui)
        {
            return new FdaDrugResponse
            {
                Results = new List<FdaDrugResult>
                {
                    new FdaDrugResult
                    {
                        OpenFda = new OpenFdaSection
                        {
                            RxCui = new List<string> { rxCui }
                        },
                        DrugInteractions = new List<string>
                        {
                            "May interact with alcohol",
                            "Use caution with other NSAIDs"
                        }
                    }
                }
            };
        }

        private FdaDrugResponse CreateMockFdaDrugResponseWithSpecificInteraction(string rxCui, string interactionText)
        {
            return new FdaDrugResponse
            {
                Results = new List<FdaDrugResult>
                {
                    new FdaDrugResult
                    {
                        OpenFda = new OpenFdaSection
                        {
                            RxCui = new List<string> { rxCui }
                        },
                        DrugInteractions = new List<string> { interactionText }
                    }
                }
            };
        }

        #endregion
    }
}