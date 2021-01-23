using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly IMessageHost _messageHost;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageHost messageHost, ILogger<MessageController> logger)
        {
            _messageHost = messageHost;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendMessage([FromBody] MessagePacket messagePacket)
        {
            messagePacket
                .VerifyNotNull(nameof(messagePacket))
                .Verify();

            await _messageHost.Send(messagePacket);
            return Ok();
        }

        [HttpPost("call")]
        public async Task<ActionResult<MessagePacket>> CallMessage([FromBody] MessagePacket messagePacket)
        {
            messagePacket
                .VerifyNotNull(nameof(messagePacket))
                .Verify();

            MessagePacket result = await _messageHost.Call(messagePacket);
            return Ok(result);
        }
    }
}
