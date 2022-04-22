using Artifact.sdk;
using Bank.sdk.Model;
using Bank.sdk.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Document;
using Toolbox.Extensions;

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
