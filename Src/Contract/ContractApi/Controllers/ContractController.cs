using Artifact.sdk;
using Contract.sdk.Models;
using Contract.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace ContractApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : Controller
    {
        private readonly ContractService _contractService;

        public ContractController(ContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Document entry, CancellationToken token)
        {
            if (!entry.IsHashVerify()) return BadRequest();

            switch (entry.ObjectClass)
            {
                case "BlkHeader":
                    BlkHeader blkHeader = entry.GetData<BlkHeader>();
                    await _contractService.Create(blkHeader, token);
                    break;

                default:
                    return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{path}")]
        public async Task<IActionResult> Delete(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            bool status = await _contractService.Delete(documentId, token: token);

            return status ? Ok() : NotFound();
        }

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            BlockChainModel? model = await _contractService.Get(documentId, token);

            return model != null ? Ok(model) : NotFound();
        }

        [HttpPost("append")]
        public async Task<IActionResult> Append([FromBody] Document entry, CancellationToken token)
        {
            if (entry.IsHashVerify()) return BadRequest();

            bool stats;
            switch (entry.ObjectClass)
            {
                case "BlkTransaction":
                    BlkTransaction blkTransaction = entry.GetData<BlkTransaction>();
                    stats = await _contractService.Append(entry.DocumentId, blkTransaction, token);
                    return stats ? Ok(stats) : NotFound();

                case "BlkCode":
                    BlkCode blkCode = entry.GetData<BlkCode>();
                    stats = await _contractService.Append(entry.DocumentId, blkCode, token);
                    return stats ? Ok(stats) : NotFound();

                default:
                    return BadRequest();
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter, CancellationToken token)
        {
            BatchSet<string> list = await _contractService.Search(queryParameter, token);
            return Ok(list);
        }
    }
}