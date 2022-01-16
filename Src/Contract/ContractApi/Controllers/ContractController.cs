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

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            BlockChainModel? model = await _contractService.Get(documentId, token);

            return model != null ? Ok(model) : NotFound();
        }

        [HttpPost("create/{path}")]
        public async Task<IActionResult> Post([FromBody] Document entry, CancellationToken token)
        {
            if( entry.IsHashVerify() ) return BadRequest();

            switch(entry.ObjectClass)
            {
                case "BlkHeader":
                    BlkHeader blkHeader = entry.GetData<BlkHeader>();
                    //await _contractService.Set(blkHeader, token);
                    break;

                default:
                    return BadRequest();
            }

            return Ok();
        }

        [HttpPost("append/{path}")]
        public async Task<IActionResult> Append([FromBody] Document entry, CancellationToken token)
        {
            if (entry.IsHashVerify()) return BadRequest();

            switch (entry.ObjectClass)
            {
                case "BlkHeader":
                    return BadRequest();

                case "BlkTransaction":
                    BlkTransaction blkTransaction = entry.GetData<BlkTransaction>();
                    //await _contractService.Set(blkTransaction, token);
                    break;

                case "BlkCode":
                    BlkCode blkCode = entry.GetData<BlkCode>();
                    //await _contractService.Set(blkCode, token);
                    break;
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

        [HttpPost("search")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter, CancellationToken token)
        {
            BatchSet<string> list = await _contractService.Search(queryParameter, token);
            return Ok(list);
        }
    }
}