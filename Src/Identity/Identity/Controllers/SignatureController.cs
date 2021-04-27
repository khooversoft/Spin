using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Store;
using Identity.sdk.Types;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Model;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignatureController : Controller
    {
        private readonly SignatureStore _store;

        public SignatureController(SignatureStore store)
        {
            _store = store;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Signature? record = await _store.Get((IdentityId)id);
            if (record == null) return NotFound();

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Signature record)
        {
            if (!record.IsValid()) return BadRequest();

            await _store.Set(record);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            bool status = await _store.Delete((IdentityId)id);
            return status ? Ok() : NotFound();
        }

        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<string> list = await _store.List(queryParameter);

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