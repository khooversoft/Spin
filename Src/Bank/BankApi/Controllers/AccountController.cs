using Bank.Abstractions.Model;
using Bank.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;

namespace BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly BankDocument _service;
        private const string _noContainer = "Container not allowed";

        public AccountController(BankHost bankHost)
        {
            _service = bankHost.BankDocument;
        }

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            if (!documentId.Container.IsEmpty()) return BadRequest(_noContainer);

            BankAccount? response = await _service.Get(documentId, token);
            if (response == null) return NotFound();

            return Ok(response);
        }

        [HttpDelete("{path}")]
        public async Task<IActionResult> Delete(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            if (!documentId.Container.IsEmpty()) return BadRequest(_noContainer);

            bool response = await _service.Delete(documentId, token);
            return response ? Ok(response) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] BankAccount bankAccount, CancellationToken token)
        {
            DocumentId documentId = (DocumentId)bankAccount.AccountId;
            if (!documentId.Container.IsEmpty()) return BadRequest(_noContainer);

            await _service.Set(bankAccount, token);
            return Ok();
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] QueryParameter queryParameter, CancellationToken token)
        {
            if (!queryParameter.Container.IsEmpty()) return BadRequest(_noContainer);

            BatchQuerySet<DatalakePathItem> response = await _service.Search(queryParameter, token);
            return Ok(response);
        }
    }
}
