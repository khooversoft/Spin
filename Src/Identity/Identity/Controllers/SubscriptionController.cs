﻿using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Subscription? record = await _subscriptionService.Get(ArtifactId.FromBase64(id));
            if (record == null) return NotFound();

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Subscription record)
        {
            if (!record.IsValid()) return BadRequest();

            await _tenantService.Set(record);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            bool status = await _tenantService.Delete(IdentityId.FromBase64(id));
            return status ? Ok() : NotFound();
        }

        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] QueryParameter queryParameter)
        {
            IReadOnlyList<string> list = await _tenantService.List(queryParameter);

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
