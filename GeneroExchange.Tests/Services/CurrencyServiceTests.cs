using GeneroProject.Model;
using GeneroProject.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CurrencyDeltaAPI.Tests.Services
{
    public class CurrencyServiceTests
    {
        private readonly Mock<IOptions<ExchangeRateApiSettings>> _mockSettings;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public CurrencyServiceTests()
        {
            _mockSettings = new Mock<IOptions<ExchangeRateApiSettings>>();
            _mockSettings.Setup(s => s.Value).Returns(new ExchangeRateApiSettings
            {
                ApiKey = "474d5da303a56627a11e3047",
                BaseUrl = "https://v6.exchangerate-api.com/v6/"
            });

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        [Fact]
        public async Task GetCurrencyDeltas_ValidRequest_ReturnsCurrencyDeltas()
        {
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"result\":\"success\",\"conversion_rates\":{\"USD\":1.0,\"SEK\":10.5}}")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            var service = new CurrencyService(_httpClient, _mockSettings.Object);
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "USD", "SEK" },
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow
            };

            var result = await service.GetCurrencyDeltas(request);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Currency == "USD" && r.Delta == 0.0m);
            Assert.Contains(result, r => r.Currency == "SEK" && r.Delta == 9.5m);
        }

        [Fact]
        public void GetCurrencyDeltas_DuplicateCurrencies_ThrowsArgumentException()
        {
            var service = new CurrencyService(_httpClient, _mockSettings.Object);
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "USD", "SEK", "SEK" },
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => service.GetCurrencyDeltas(request));
            Assert.Equal("Currencies must be unique.", exception.Result.Message);
        }

        [Fact]
        public void GetCurrencyDeltas_InvalidDateRange_ThrowsArgumentException()
        {
            var service = new CurrencyService(_httpClient, _mockSettings.Object);
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "USD", "SEK" },
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow.AddDays(-1)
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => service.GetCurrencyDeltas(request));
            Assert.Equal("To date must be greater than from date.", exception.Result.Message);
        }
    }
}
