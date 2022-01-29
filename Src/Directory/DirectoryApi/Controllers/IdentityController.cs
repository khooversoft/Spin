using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DirectoryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly IdentityService _identityService;

    public IdentityController(IdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] IdentityEntryRequest identityEntryRequest, CancellationToken token)
    {
        bool success = await _identityService.Create(identityEntryRequest, token);
        if (!success) return Conflict();
        return Ok();
    }

    [HttpDelete("{path}")]
    public async Task<IActionResult> Delete(string path, CancellationToken token)
    {
        if (path.IsDocumentIdValid().Valid) return BadRequest();
        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

        bool status = await _identityService.Delete(documentId, token);

        return status ? Ok() : NotFound();
    }

    [HttpGet("{path}")]
    public async Task<IActionResult> Get(string path, CancellationToken token)
    {
        if (path.IsDocumentIdValid().Valid) return BadRequest();
        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

        bool bypassCache = Request.Headers.ContainsKey(Constants.BypassCacheName);

        IdentityEntry? entry = await _identityService.Get(documentId, token, bypassCache: bypassCache);
        if (entry == null) return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Set([FromBody] IdentityEntry entry, CancellationToken token)
    {
        await _identityService.Set(entry, token);
        return Ok();
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] QueryParameter queryParameter, CancellationToken token)
    {
        IReadOnlyList<DatalakePathItem> list = await _identityService.Search(queryParameter);

        var result = new BatchSet<DatalakePathItem>
        {
            QueryParameter = queryParameter,
            NextIndex = queryParameter.Index + queryParameter.Count,
            Records = list.ToArray(),
        };

        return Ok(result);
    }
}
