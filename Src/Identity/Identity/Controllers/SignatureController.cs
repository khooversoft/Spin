using ArtifactStore.sdk.Actors;
using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Services;
using Identity.sdk.Types;
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
    public class SignatureController : Controller
    {
        private readonly SignatureService _signatureService;

        public SignatureController(SignatureService signatureService)
        {
            _signatureService = signatureService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Signature? record = await _signatureService.Get((IdentityId)id);
            if (record == null) return NotFound();

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Signature record)
        {
            if (!record.IsValid()) return BadRequest();

            await _signatureService.Set(record);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            bool status = await _signatureService.Delete((IdentityId)id);
            return status ? Ok() : NotFound();
        }

        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<string> list = await _signatureService.List(queryParameter);

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
