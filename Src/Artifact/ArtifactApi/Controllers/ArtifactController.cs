using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;

namespace Artifact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtifactController : Controller
    {
        private readonly IDocumentPackage _documentStore;

        public ArtifactController(IDocumentPackage documentStore)
        {
            _documentStore = documentStore;
        }

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            Document? document = await _documentStore.Get(documentId, token);

            return document == null ? NotFound() : Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Document entry, CancellationToken token)
        {
            await _documentStore.Set(entry, token: token);
            return Ok();
        }


        [HttpDelete("{path}")]
        public async Task<IActionResult> Delete(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            bool status = await _documentStore.Delete(documentId, token: token);

            return status ? Ok() : NotFound();
        }

        [HttpPost("search")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<DatalakePathItem> list = await _documentStore.Search(queryParameter);

            var result = new BatchSet<string>
            {
                QueryParameter = queryParameter,
                NextIndex = queryParameter.Index + queryParameter.Count,
                Records = list.Select(x => x.Name).ToArray(),
            };

            return Ok(result);
        }
    }
}