using GeneroProject.Model;
using GeneroProject.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeneroProject.Repositories
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRateApiSettings _settings;

        public CurrencyService(HttpClient httpClient, IOptions<ExchangeRateApiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<List<CurrencyDelta>> GetCurrencyDeltas(CurrencyRequest request)
        {
            ValidateRequest(request);

            var baseRates = await GetLatestExchangeRates(request.BaseCurrency);
            var deltas = new List<CurrencyDelta>();

            foreach (var currency in request.Currencies)
            {
                if (!baseRates.ContainsKey(currency))
                {
                    throw new ArgumentException($"Currency {currency} does not exist.");
                }

                var rate = baseRates[currency];
                var delta = Math.Round(rate - 1.0m, 3);
                deltas.Add(new CurrencyDelta { Currency = currency, Delta = delta });
            }

            return deltas;
        }

        private void ValidateRequest(CurrencyRequest request)
        {
            if (request.Currencies.Distinct().Count() != request.Currencies.Count)
            {
                throw new ArgumentException("Currencies must be unique.");
            }

            if (request.FromDate >= request.ToDate)
            {
                throw new ArgumentException("To date must be greater than from date.");
            }
        }

        private async Task<Dictionary<string, decimal>> GetLatestExchangeRates(string baseCurrency)
        {
            var url = $"{_settings.BaseUrl}{_settings.ApiKey}/latest/{baseCurrency}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error fetching data: {response.StatusCode}, Content: {content}");
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ExchangeRateApiResponse>(responseData);

            if (result == null || result.ConversionRates == null)
            {
                throw new Exception("Invalid response structure from the API");
            }

            return result.ConversionRates;
        }
    }
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; }
    }
}
