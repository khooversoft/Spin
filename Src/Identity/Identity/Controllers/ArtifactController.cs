using ArtifactStore.sdk.Actors;
using ArtifactStore.sdk.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Model;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : Controller
    {
        private readonly IArtifactStoreService _acticleStoreService;

        public IdentityController(IArtifactStoreService acticleStoreService)
        {
            _acticleStoreService = acticleStoreService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            ArtifactPayload? record = await _acticleStoreService.Get(ArtifactId.FromBase64(id));
            if (record == null) return NotFound();

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ArtifactPayload record)
        {
            if (!record.IsValid()) return BadRequest();

            await _acticleStoreService.Set(record);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            bool status = await _acticleStoreService.Delete(ArtifactId.FromBase64(id));
            return status ? Ok() : NotFound();
        }

        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<string> list = await _acticleStoreService.List(queryParameter);

            var result = new BatchSet<string>
            {
                QueryParameter = queryParameter,
                NextIndex = queryParameter.Index + queryParameter.Count,
                Records = list.ToArray(),
            };

            return Ok(result);
        }
    }
}
