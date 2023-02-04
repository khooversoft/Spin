using Directory.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Abstractions.Protocol;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DirectoryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EntryController : ControllerBase
{
    private readonly DirectoryService _directoryService;

    public EntryController(DirectoryService directoryService)
    {
        _directoryService = directoryService;
    }


    [HttpGet("{path}")]
    public async Task<IActionResult> Get(string path, CancellationToken token)
    {
        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);

        bool bypassCache = Request.Headers.ContainsKey(Constants.BypassCacheName);

        DirectoryEntry? entry = await _directoryService.Get(documentId, token, bypassCache: bypassCache);
        if (entry == null) return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] DirectoryEntry entry, CancellationToken token)
    {
        await _directoryService.Set(entry, token);
        return Ok();
    }

    [HttpDelete("{path}")]
    public async Task<IActionResult> Delete(string path, CancellationToken token)
    {
        DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
        bool status = await _directoryService.Delete(documentId, token);

        return status ? Ok() : NotFound();
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] QueryParameter queryParameter, CancellationToken token)
    {
        IReadOnlyList<DatalakePathItem> list = await _directoryService.Search(queryParameter);

        var result = new BatchQuerySet<DatalakePathItem>
        {
            QueryParameter = queryParameter,
            NextIndex = queryParameter.Index + queryParameter.Count,
            Records = list.ToArray(),
        };

        return Ok(result);
    }
}
