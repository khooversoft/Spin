using Contract.sdk.Models;
using Contract.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Model;

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

        return model != null ? Ok(model) : NotFound();
    }

    [HttpGet("latest/{path}/{blockType}")]
    public async Task<IActionResult> GetLatest(string path, string blockType, CancellationToken token)
    {
        if (path.IsEmpty()) return BadRequest();
        if (blockType.IsEmpty()) return BadRequest();

        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
        Document? document = await _contractService.GetLatest(documentId, blockType, token);

        return document switch
        {
            null => NotFound(),
            _ => Ok(document),
        };
    }

    [HttpGet("all/{path}/{blockType}")]
    public async Task<IActionResult> GetAll(string path, string blockType, CancellationToken token)
    {
        if (path.IsEmpty()) return BadRequest();
        if (blockType.IsEmpty()) return BadRequest();

        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
        IReadOnlyList<Document>? document = await _contractService.GetAll(documentId, blockType, token);

        return document switch
        {
            null => NotFound(),
            _ => Ok(document),
        };
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

        BatchSet<string> result = new BatchSet<string>()
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
    public async Task<IActionResult> Append([FromBody] Document entry, CancellationToken token)
    {
        if (!entry.IsHashVerify()) return BadRequest();

        bool result = await _contractService.Append(entry, token);
        return result switch
        {
            true => Ok(),
            false => BadRequest(),
        };
    }

    [HttpPost("validate/{path}")]
    public async Task<IActionResult> Validate(string path, CancellationToken token)
    {
        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

        bool isValid = await _contractService.Validate(documentId, token);
        return isValid ? Ok() : Conflict();
    }
}