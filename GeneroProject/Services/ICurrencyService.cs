using GeneroProject.Model;

namespace GeneroProject.Services
{
    public interface ICurrencyService
    {
        Task<List<CurrencyDelta>> GetCurrencyDeltas(CurrencyRequest request);
    }
}
