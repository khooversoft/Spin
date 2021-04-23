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
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{tenantId}/{subscriptionId}/{id}")]
        public async Task<IActionResult> Get(string tenantId, string subscriptionId, string id)
        {
            User? record = await _userService.Get((IdentityId)tenantId, (IdentityId)subscriptionId, (UserId)id);
            if (record == null) return NotFound();

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User record)
        {
            if (!record.IsValid()) return BadRequest();

            await _userService.Set(record);
            return Ok();
        }

        [HttpDelete("{tenantId}/{subscriptionId}/{id}")]
        public async Task<IActionResult> Delete(string tenantId, string subscriptionId, string id)
        {
            bool status = await _userService.Delete((IdentityId)tenantId, (IdentityId)subscriptionId, (UserId)id);
            return status ? Ok() : NotFound();
        }

        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<string> list = await _userService.List(queryParameter);

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
