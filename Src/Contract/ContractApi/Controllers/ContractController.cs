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


        //  ///////////////////////////////////////////////////////////////////////////////////////
        //  CRUD

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            BlockChainModel? model = await _contractService.Get(documentId, token);

            return model != null ? Ok(model) : NotFound();
        }

        [HttpPost("set/{path}")]
        public async Task<IActionResult> Set(string path, [FromBody] BlockChainModel blockChainModel, CancellationToken token)
        {
            blockChainModel.Verify();
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

            bool isValid = await _contractService.Validate(blockChainModel.ToBlockChain(), token);
            if (!isValid) return Conflict();

            await _contractService.Set(documentId, blockChainModel, token);
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
            BatchSet<DatalakePathItem> list = await _contractService.Search(queryParameter, token);
            return Ok(list);
        }


        //  ///////////////////////////////////////////////////////////////////////////////////////
        //  Block chain

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Document entry, CancellationToken token)
        {
            if (!entry.IsHashVerify()) return BadRequest();

            switch (entry.ObjectClass)
            {
                case "BlkHeader":
                    BlkHeader blkHeader = entry.DeserializeData<BlkHeader>();
                    await _contractService.Create(blkHeader, token);
                    break;

                default:
                    return BadRequest();
            }

            return Ok();
        }

        [HttpPost("append")]
        public async Task<IActionResult> Append([FromBody] Document entry, CancellationToken token)
        {
            if (entry.IsHashVerify()) return BadRequest();

            bool stats;
            switch (entry.ObjectClass)
            {
                case "BlkTransaction":
                    BlkTransaction blkTransaction = entry.DeserializeData<BlkTransaction>();
                    stats = await _contractService.Append(entry.DocumentId, blkTransaction, token);
                    return stats ? Ok(stats) : NotFound();

                case "BlkCode":
                    BlkCode blkCode = entry.DeserializeData<BlkCode>();
                    stats = await _contractService.Append(entry.DocumentId, blkCode, token);
                    return stats ? Ok(stats) : NotFound();

                default:
                    return BadRequest();
            }
        }

        [HttpPost("sign/{path}")]
        public async Task<IActionResult> Sign(string path, [FromBody] BlockChainModel blockChainModel, CancellationToken token)
        {
            blockChainModel.Verify();

            BlockChain blockChain = await _contractService.Sign(blockChainModel.ToBlockChain(), token);

            if(path == "model")
            {
                return Ok(blockChain.ToBlockChainModel());
            }

            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            await _contractService.Set(documentId, blockChain.ToBlockChainModel(), token);
            return Ok();
        }

        [HttpPost("validate/{path}")]
        public async Task<IActionResult> Validate(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

            bool isValid = await _contractService.Validate(documentId, token);
            return isValid ? Ok() : Conflict();
        }


        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] BlockChainModel blockChainModel, CancellationToken token)
        {
            blockChainModel.Verify();

            bool isValid = await _contractService.Validate(blockChainModel.ToBlockChain(), token);
            return isValid ? Ok() : Conflict();
        }
    }
}