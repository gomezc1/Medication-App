using FluentAssertions;
using MedicationManager.Core.Models.Exceptions;
using MedicationManager.Infrastructure.ExternalServices;
using MedicationManager.Infrastructure.ExternalServices.Models.RxNorm;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace MedicationManager.Tests.ExternalServices
{
    public class RxNormApiServiceTests
    {
        private readonly Mock<ILogger<RxNormApiService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly RxNormApiService _service;

        public RxNormApiServiceTests()
        {
            _mockLogger = new Mock<ILogger<RxNormApiService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://rxnav.nlm.nih.gov/REST/")
            };
            _service = new RxNormApiService(_httpClient, _mockLogger.Object);
        }

        #region SearchApproximateMatchAsync Tests

        [Fact]
        public async Task SearchApproximateMatchAsync_WithValidTerm_ReturnsCandidates()
        {
            // Arrange
            var searchTerm = "tylenol";
            var expectedResponse = CreateMockRxNormApproximateResponse("tylenol", "161", 100);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchApproximateMatchAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.First().Name.ToLower().Should().Contain("tylenol");
            result.First().RxCui.Should().Be("161");
        }

        [Fact]
        public async Task SearchApproximateMatchAsync_WithMultipleResults_ReturnsOrderedByScore()
        {
            // Arrange
            var searchTerm = "aspirin";
            var expectedResponse = CreateMockRxNormApproximateResponseMultiple();

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchApproximateMatchAsync(searchTerm);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCountGreaterThan(1);
            result.First().Score.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task SearchApproximateMatchAsync_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            var searchTerm = "nonexistentdrug12345";
            var expectedResponse = new RxNormApproximateResponse
            {
                ApproximateGroup = new RxNormApproximateGroup
                {
                    Candidates = new List<RxNormCandidate>()
                }
            };

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.SearchApproximateMatchAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchApproximateMatchAsync_WithHttpRequestException_ThrowsApiException()
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
            await Assert.ThrowsAsync<ApiException>(() => _service.SearchApproximateMatchAsync(searchTerm));
        }

        [Fact]
        public async Task SearchApproximateMatchAsync_WithTimeout_ThrowsApiException()
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
            await Assert.ThrowsAsync<ApiException>(() => _service.SearchApproximateMatchAsync(searchTerm));
        }

        #endregion

        #region GetRxCuiDetailsAsync Tests

        [Fact]
        public async Task GetRxCuiDetailsAsync_WithValidRxCui_ReturnsDetails()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = CreateMockRxNormPropertiesResponse(rxCui, "Acetaminophen");

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetRxCuiDetailsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result!.RxCui.Should().Be(rxCui);
            result.Name.Should().Be("Acetaminophen");
        }

        [Fact]
        public async Task GetRxCuiDetailsAsync_WithInvalidRxCui_ReturnsNull()
        {
            // Arrange
            var rxCui = "99999999";
            var expectedResponse = new RxNormPropertiesResponse
            {
                Properties = null
            };
            SetupHttpResponse(HttpStatusCode.NotFound, expectedResponse);

            // Act
            var result = await _service.GetRxCuiDetailsAsync(rxCui);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetRxCuiDetailsAsync_WithException_ReturnsNull()
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
            var result = await _service.GetRxCuiDetailsAsync(rxCui);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetActiveIngredientsAsync Tests

        [Fact]
        public async Task GetActiveIngredientsAsync_WithValidRxCui_ReturnsIngredients()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = CreateMockRxNormRelatedResponseWithIngredients(rxCui, new[] { "Acetaminophen" });

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetActiveIngredientsAsync(rxCui);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().Contain("Acetaminophen");
        }

        [Fact]
        public async Task GetActiveIngredientsAsync_WithMultipleIngredients_ReturnsAllIngredients()
        {
            // Arrange
            var rxCui = "1234";
            var ingredients = new[] { "Acetaminophen", "Caffeine", "Aspirin" };
            var expectedResponse = CreateMockRxNormRelatedResponseWithIngredients(rxCui, ingredients);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetActiveIngredientsAsync(rxCui);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(ingredients);
        }

        [Fact]
        public async Task GetActiveIngredientsAsync_WithNoIngredients_ReturnsEmptyList()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = new RxNormRelatedResponse
            {
                RelatedGroup = new RxNormRelatedGroup
                {
                    ConceptGroup = new List<RxNormConceptGroup>()
                }
            };

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetActiveIngredientsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActiveIngredientsAsync_WithException_ReturnsEmptyList()
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
            var result = await _service.GetActiveIngredientsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetDrugClassesAsync Tests

        [Fact]
        public async Task GetDrugClassesAsync_WithValidRxCui_ReturnsList()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = new StringContent("{}");

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = expectedResponse
                });

            // Act
            var result = await _service.GetDrugClassesAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDrugClassesAsync_WithException_ReturnsEmptyList()
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
            var result = await _service.GetDrugClassesAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetRelatedDrugsAsync Tests

        [Fact]
        public async Task GetRelatedDrugsAsync_WithValidRxCui_ReturnsRelatedDrugs()
        {
            // Arrange
            var rxCui = "161";
            var expectedResponse = CreateMockRxNormRelatedResponse(rxCui);

            SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _service.GetRelatedDrugsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetRelatedDrugsAsync_WithException_ReturnsEmptyList()
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
            var result = await _service.GetRelatedDrugsAsync(rxCui);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Helper Methods

        private void SetupHttpResponse<T>(HttpStatusCode statusCode, T? response)
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

        private RxNormApproximateResponse CreateMockRxNormApproximateResponse(string name, string rxCui, int score)
        {
            return new RxNormApproximateResponse
            {
                ApproximateGroup = new RxNormApproximateGroup
                {
                    Candidates = new List<RxNormCandidate>
                    {
                        new RxNormCandidate
                        {
                            RxCui = rxCui,
                            Name = name,
                            ScoreString = score.ToString()
                        }
                    }
                }
            };
        }

        private RxNormApproximateResponse CreateMockRxNormApproximateResponseMultiple()
        {
            return new RxNormApproximateResponse
            {
                ApproximateGroup = new RxNormApproximateGroup
                {
                    Candidates =
                    [
                        new() { RxCui = "1191", Name = "Aspirin", ScoreString = "100" },
                        new() { RxCui = "1192", Name = "Aspirin 325mg", ScoreString = "95" },
                        new() { RxCui = "1193", Name = "Aspirin 81mg", ScoreString = "90" }
                    ]
                }
            };
        }

        private RxNormPropertiesResponse CreateMockRxNormPropertiesResponse(string rxCui, string name)
        {
            return new RxNormPropertiesResponse
            {
                Properties = new RxNormProperties
                {
                    RxCui = rxCui,
                    Name = name,
                    Tty = "IN"
                }
            };
        }

        private RxNormRelatedResponse CreateMockRxNormRelatedResponseWithIngredients(string rxCui, string[] ingredients)
        {
            var conceptProperties = ingredients.Select(ingredient => new RxNormConceptProperty
            {
                RxCui = rxCui,
                Name = ingredient,
                Tty = "IN"
            }).ToList();

            return new RxNormRelatedResponse
            {
                RelatedGroup = new RxNormRelatedGroup
                {
                    ConceptGroup = new List<RxNormConceptGroup>
                    {
                        new RxNormConceptGroup
                        {
                            Tty = "IN",
                            ConceptProperties = conceptProperties
                        }
                    }
                }
            };
        }

        private RxNormRelatedResponse CreateMockRxNormRelatedResponse(string rxCui)
        {
            return new RxNormRelatedResponse
            {
                RelatedGroup = new RxNormRelatedGroup
                {
                    ConceptGroup = new List<RxNormConceptGroup>
                    {
                        new RxNormConceptGroup
                        {
                            Tty = "SCD",
                            ConceptProperties = new List<RxNormConceptProperty>
                            {
                                new RxNormConceptProperty
                                {
                                    RxCui = rxCui,
                                    Name = "Related Drug",
                                    Tty = "SCD"
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion
    }
}