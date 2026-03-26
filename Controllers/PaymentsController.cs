using FootballBooking_BE.Common;
using FootballBooking_BE.Models;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FootballBooking_BE.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOptions<SePaySettings> _sePaySettings;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService, 
            IOptions<SePaySettings> sePaySettings,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _sePaySettings = sePaySettings;
            _logger = logger;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SePayWebhook([FromBody] System.Text.Json.JsonElement rawPayload)
        {
            _logger.LogInformation("Receiving SePay Webhook request.");

            SePayWebhookPayload payload;
            try {
                payload = System.Text.Json.JsonSerializer.Deserialize<SePayWebhookPayload>(rawPayload.GetRawText())!;
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to deserialize SePay webhook payload.");
                return BadRequest(new { message = "Invalid payload format" });
            }

            // Security Check: Verify Webhook Token
            var authHeader = Request.Headers["Authorization"].ToString();
            var expectedToken = _sePaySettings.Value.WebhookToken;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.Contains(expectedToken))
            {
                _logger.LogWarning("SePay Webhook: Unauthorized access attempt.");
                return Unauthorized(new { message = "Invalid webhook token" });
            }

            try 
            {
                var result = await _paymentService.ProcessWebhookAsync(payload);
                return Ok(new { status = result ? "success" : "skipped" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay Webhook.");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("sync-sepay")]
        [Authorize]
        public async Task<IActionResult> SyncSePay()
        {
            try
            {
                var updatedCount = await _paymentService.SyncSePayTransactionsAsync();
                return Ok(ApiResponse<object>.Ok(new { updatedCount }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing SePay transactions.");
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("info")]
        [AllowAnonymous]
        public IActionResult GetPaymentInfo()
        {
            var data = new { 
                accountNumber = _sePaySettings.Value.AccountNumber,
                bankName = _sePaySettings.Value.BankName,
                accountName = "FOOTBALL BOOKING" 
            };
            return Ok(ApiResponse<object>.Ok(data));
        }
    }
}
