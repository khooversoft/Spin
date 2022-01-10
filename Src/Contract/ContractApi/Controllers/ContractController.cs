using Artifact.sdk;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Model;

namespace ContractApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : Controller
    {
        private readonly IArtifactClient _artifactClient;

        public ContractController(IArtifactClient artifactClient)
        {
            _artifactClient = artifactClient;
        }

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            Document? document = await _artifactClient.Get(documentId, token);
            if (document == null) return NotFound();

            BlockChainModel model = document.GetData<BlockChainModel>();
            return Ok(model);
        }

        [HttpPost("create/{path}")]
        public async Task<IActionResult> Post([FromBody] Document entry, CancellationToken token)
        {
            //await _documentStore.Set(entry, token: token);
            return Ok();
        }


        [HttpDelete("{path}")]
        public async Task<IActionResult> Delete(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            //bool status = await _documentStore.Delete(documentId, token: token);

            //return status ? Ok() : NotFound();
            return Ok();
        }

        [HttpPost("search")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            //IReadOnlyList<DatalakePathItem> list = await _documentStore.Search(queryParameter);

            //var result = new BatchSet<string>
            //{
            //    QueryParameter = queryParameter,
            //    NextIndex = queryParameter.Index + queryParameter.Count,
            //    Records = list.Select(x => x.Name).ToArray(),
            //};

            //return Ok(result);
            return Ok();
        }
    }
}