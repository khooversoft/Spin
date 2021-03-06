﻿using MessageNet.sdk.Endpoint;
using MessageNet.sdk.Host;
using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace MessageNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : Controller
    {
        private readonly MessageEndpointCollection _messageEndpointCollection;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(MessageEndpointCollection messageEndpointCollection, ILogger<RegisterController> logger)
        {
            _messageEndpointCollection = messageEndpointCollection;
            _logger = logger;
        }

        [HttpPost()]
        public IActionResult Register([FromBody] RegisterSync registerSync)
        {
            _logger.LogTrace($"{nameof(Register)}: Registering endpointId={registerSync.EndpointId}, callbackUri={registerSync.CallbackUri}");

            if (!registerSync.IsValid()) return BadRequest();

            return _messageEndpointCollection.Register(registerSync.EndpointId, new Uri(registerSync.CallbackUri)) switch
            {
                true => Ok(),

                _ => Conflict("Registration already exist"),
            };
        }

        [HttpDelete("{endpointId}")]
        public async Task<IActionResult> Remove(string endpointId)
        {
            if (endpointId.IsEmpty()) return BadRequest("Missing endpointId");

            EndpointId ep;
            try { ep = EndpointId.FromBase64(endpointId); }
            catch (Exception ex)
            {
                _logger.LogError($"Invalid endpoint id", ex);
                return BadRequest();
            }

            return await _messageEndpointCollection.Remove(ep) switch
            {
                true => Ok(),

                _ => NotFound($"EndpointId={ep} was not registered"),
            };
        }
    }
}
