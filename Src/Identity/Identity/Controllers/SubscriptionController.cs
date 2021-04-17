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
    public class SubscriptionController : Controller
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            (IdentityId tenantId, IdentityId subscriptionId) = Subscription.ParseId(ArtifactId.FromBase64(id));

            Subscription? record = await _subscriptionService.Get(tenantId, subscriptionId);
            if (record == null) return NotFound();

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Subscription record)
        {
            if (!record.IsValid()) return BadRequest();

            await _subscriptionService.Set(record);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            (IdentityId tenantId, IdentityId subscriptionId) = Subscription.ParseId(ArtifactId.FromBase64(id));

            bool status = await _subscriptionService.Delete(tenantId, subscriptionId);
            return status ? Ok() : NotFound();
        }

        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<string> list = await _subscriptionService.List(queryParameter);

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
