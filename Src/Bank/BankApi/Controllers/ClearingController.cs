using Bank.Abstractions.Model;
using Bank.sdk.Service;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClearingController : ControllerBase
    {
        private readonly BankClearing _client;

        public ClearingController(BankHost bankHost)
        {
            _client = bankHost.BankClearing;
        }

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] TrxBatch<TrxRequest> batch, CancellationToken token)
        {
            if (batch.Items.Count == 0) return BadRequest("Batch is empty");

            TrxBatch<TrxRequestResponse> result = await _client.Send(batch, token);
            return Ok(result);
        }
    }
}
