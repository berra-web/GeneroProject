using GeneroProject.Controllers;
using GeneroProject.Model;
using GeneroProject.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CurrencyDeltaAPI.Tests.Controllers
{
    public class CurrencyControllerTests
    {
        private readonly Mock<ICurrencyService> _mockCurrencyService;
        private readonly CurrencyController _controller;

        public CurrencyControllerTests()
        {
            _mockCurrencyService = new Mock<ICurrencyService>();
            _controller = new CurrencyController(_mockCurrencyService.Object);
        }

        [Fact]
        public async Task GetCurrencyDeltas_ValidRequest_ReturnsOkResult()
        {
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "USD", "SEK" },
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow
            };

            var deltas = new List<CurrencyDelta>
            {
                new CurrencyDelta { Currency = "USD", Delta = 0.0m },
                new CurrencyDelta { Currency = "SEK", Delta = 9.5m }
            };

            _mockCurrencyService.Setup(s => s.GetCurrencyDeltas(request)).ReturnsAsync(deltas);

            var result = await _controller.GetCurrencyDeltas(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<CurrencyDelta>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, r => r.Currency == "USD" && r.Delta == 0.0m);
            Assert.Contains(returnValue, r => r.Currency == "SEK" && r.Delta == 9.5m);
        }

        [Fact]
        public async Task GetCurrencyDeltas_DuplicateCurrencies_ReturnsBadRequest()
        {
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "SEK", "SEK" },
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow
            };

            _mockCurrencyService.Setup(s => s.GetCurrencyDeltas(request)).ThrowsAsync(new ArgumentException("Currencies must be unique."));

            var result = await _controller.GetCurrencyDeltas(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("InvalidRequest", errorResponse.ErrorCode);
            Assert.Equal("Currencies must be unique.", errorResponse.ErrorDetails);
        }

        [Fact]
        public async Task GetCurrencyDeltas_InvalidDateRange_ReturnsBadRequest()
        {
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "USD", "SEK" },
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockCurrencyService.Setup(s => s.GetCurrencyDeltas(request)).ThrowsAsync(new ArgumentException("To date must be greater than from date."));

            var result = await _controller.GetCurrencyDeltas(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("InvalidRequest", errorResponse.ErrorCode);
            Assert.Equal("To date must be greater than from date.", errorResponse.ErrorDetails);
        }

        [Fact]
        public async Task GetCurrencyDeltas_ApiError_ReturnsInternalServerError()
        {
            var request = new CurrencyRequest
            {
                BaseCurrency = "USD",
                Currencies = new List<string> { "USD", "SEK" },
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow
            };

            _mockCurrencyService.Setup(s => s.GetCurrencyDeltas(request)).ThrowsAsync(new HttpRequestException("API Error"));

            var result = await _controller.GetCurrencyDeltas(request);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerErrorResult.StatusCode);
            var errorResponse = Assert.IsType<ErrorResponse>(internalServerErrorResult.Value);
            Assert.Equal("ApiError", errorResponse.ErrorCode);
            Assert.Equal("API Error", errorResponse.ErrorDetails);
        }
    }
}
