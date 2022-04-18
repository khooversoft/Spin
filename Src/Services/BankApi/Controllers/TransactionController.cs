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
    public class TransactionController : ControllerBase
    {
        private readonly BankTransaction _service;
        private const string _noContainer = "Container not allowed";

        public TransactionController(BankHost bankHost)
        {
            _service = bankHost.Transaction;
        }

        [HttpGet("balance/{path}")]
        public async Task<IActionResult> GetBalance(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            if (!documentId.Container.IsEmpty()) return BadRequest(_noContainer);

            TrxBalance? response = await _service.GetBalance(documentId, token);
            if (response == null) return NotFound();

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] TrxBatch<TrxRequest> batch, CancellationToken token)
        {
            if (batch.Items.Count == 0) return BadRequest("Batch is empty");

            TrxBatch<TrxRequestResponse> response = await _service.Set(batch, token);
            return Ok(response);
        }
    }
}
