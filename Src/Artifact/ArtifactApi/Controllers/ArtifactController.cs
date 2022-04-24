using ArtifactApi.Application;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;

namespace Artifact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtifactController : Controller
    {
        private const string _containerErrorMsg = "Container required";
        private readonly DocumentPackageFactory _documentPackageFactory;

        public ArtifactController(DocumentPackageFactory documentPackage)
        {
            _documentPackageFactory = documentPackage;
        }

        [HttpGet("{path}")]
        public async Task<IActionResult> Get(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            if (documentId.Container.IsEmpty() || !_documentPackageFactory.Exist(documentId.Container)) return BadRequest(_containerErrorMsg);

            Document? document = await _documentPackageFactory
                .Create(documentId.Container)
                .Get(documentId, token);

            return document == null ? NotFound() : Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] Document entry, CancellationToken token)
        {
            DocumentId documentId = (DocumentId)entry.DocumentId;
            if (documentId.Container.IsEmpty() || !_documentPackageFactory.Exist(documentId.Container)) return BadRequest(_containerErrorMsg);

            await _documentPackageFactory
                .Create(documentId.Container)
                .Set(entry, token: token);

            return Ok();
        }


        [HttpDelete("{path}")]
        public async Task<IActionResult> Delete(string path, CancellationToken token)
        {
            DocumentId documentId = DocumentIdTools.FromUrlEncoding(path);
            if (documentId.Container.IsEmpty() || !_documentPackageFactory.Exist(documentId.Container)) return BadRequest(_containerErrorMsg);

            bool status = await _documentPackageFactory
                .Create(documentId.Container)
                .Delete(documentId, token: token);

            return status ? Ok() : NotFound();
        }

        [HttpPost("search")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            if (queryParameter.Container.IsEmpty() || !_documentPackageFactory.Exist(queryParameter.Container)) return BadRequest(_containerErrorMsg);

            IReadOnlyList<DatalakePathItem> list = await _documentPackageFactory
                .Create(queryParameter.Container)
                .Search(queryParameter);

            var result = new BatchSet<DatalakePathItem>
            {
                QueryParameter = queryParameter,
                NextIndex = queryParameter.Index + queryParameter.Count,
                Records = list.ToArray(),
            };

            return Ok(result);
        }
    }
}