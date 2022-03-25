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
        private readonly BankClearingQueue _client;

        public ClearingController(BankClearingQueue client)
        {
            _client = client;
        }

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] TrxBatch<TrxRequest> batch, CancellationToken token)
        {
            if (batch.Items.Count == 0) return BadRequest("Batch is empty");

            await _client.Send(batch, token);
            return Ok();
        }
    }
}
