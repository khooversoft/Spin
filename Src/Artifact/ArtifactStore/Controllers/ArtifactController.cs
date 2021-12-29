//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using ArtifactStore.sdk.Model;
//using ArtifactStore.sdk.Services;
//using Microsoft.AspNetCore.Mvc;
//using Toolbox.Extensions;
//using Toolbox.Model;

//namespace ArtifactStore.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ArtifactController : Controller
//    {
//        private readonly IArtifactStoreFactory _acticleStoreFactory;

//        public ArtifactController(IArtifactStoreFactory acticleStoreFactory)
//        {
//            _acticleStoreFactory = acticleStoreFactory;
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(string id)
//        {
//            ArtifactId artifactId = ArtifactId.FromBase64(id);

//            ArtifactPayload? record = await _acticleStoreFactory.Create(artifactId).Get(artifactId);

//            return record == null ? NotFound() : Ok(record);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] ArtifactPayload record)
//        {
//            var (valid, _) = record.IsValid();
//            if (valid == false) return BadRequest();

//            await _acticleStoreFactory.Create(new ArtifactId(record.Id)).Set(record);
//            return Ok();
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(string id)
//        {
//            ArtifactId artifactId = ArtifactId.FromBase64(id);

//            bool status = await _acticleStoreFactory.Create(artifactId).Delete(artifactId);

//            return status ? Ok() : NotFound();
//        }

//        [HttpPost("list")]
//        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
//        {
//            if (queryParameter.Namespace.IsEmpty()) return BadRequest();

//            IReadOnlyList<string> list = await _acticleStoreFactory.Create(queryParameter.Namespace).List(queryParameter);

//            var result = new BatchSet<string>
//            {
//                QueryParameter = queryParameter,
//                NextIndex = queryParameter.Index + queryParameter.Count,
//                Records = list.ToArray(),
//            };

//            return Ok(result);
//        }
//    }
//}