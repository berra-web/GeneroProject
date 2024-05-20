using GeneroProject.Model;
using GeneroProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeneroProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpPost("GetCurrencyDeltas")]
        public async Task<IActionResult> GetCurrencyDeltas([FromBody] CurrencyRequest request)
        {
            try
            {
                var deltas = await _currencyService.GetCurrencyDeltas(request);
                return Ok(deltas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorCode = "InvalidRequest",
                    ErrorDetails = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    ErrorCode = "ApiError",
                    ErrorDetails = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    ErrorCode = "InternalServerError",
                    ErrorDetails = ex.Message
                });
            }
        }
    }
}
