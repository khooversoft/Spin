using Contract.sdk.Models;
using Contract.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Monads;

namespace ContractApi.Controllers;

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

        return model switch
        {
            null => NotFound(),
            _ => Ok(model)
        };
    }

    [HttpGet("{path}/{blockTypes}")]
    public async Task<IActionResult> GetCollections(string path, string blockTypes, CancellationToken token)
    {
        if (path.IsEmpty()) return BadRequest();
        if (blockTypes.IsEmpty()) return BadRequest();

        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

        return (await _contractService.Get(documentId, blockTypes, token))
            .Switch<IActionResult>(x => Ok(x), () => NotFound());
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
        BatchQuerySet<DatalakePathItem> list = await _contractService.Search(queryParameter, token);

        BatchQuerySet<string> result = new BatchQuerySet<string>()
        {
            QueryParameter = list.QueryParameter,
            NextIndex = list.NextIndex,
            Records = list.Records.Select(x => x.Name).ToArray()
        };

        return Ok(result);
    }


    //  ///////////////////////////////////////////////////////////////////////////////////////
    //  Block chain

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ContractCreateModel contractCreate, CancellationToken token)
    {
        await _contractService.Create(contractCreate, token);
        return Ok();
    }

    [HttpPost("append")]
    public async Task<IActionResult> Append([FromBody] Batch<Document> batch, CancellationToken token)
    {
        if (batch.Items.Count == 0) return BadRequest();
        if (batch.Items.Any(x => !x.IsHashVerify())) return BadRequest();

        AppendResult result = await _contractService.Append(batch, token);
        return Ok(result);
    }

    [HttpPost("validate/{path}")]
    public async Task<IActionResult> Validate(string path, CancellationToken token)
    {
        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

        bool isValid = await _contractService.Validate(documentId, token);
        return isValid ? Ok() : Conflict();
    }
}