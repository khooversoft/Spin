using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Toolbox.Logging;
using Toolbox.Model;

namespace ArtifactStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        private readonly LoggerBuffer _loggerBuffer;

        public PingController(LoggerBuffer loggerBuffer)
        {
            _loggerBuffer = loggerBuffer;
        }

        /// <summary>
        /// Get the last 100 logs in reverse order
        /// </summary>
        /// <returns>array of strings</returns>
        /// 
        [Description("Return the last 100 internal operation logs")]
        [HttpGet("Logs")]
        public ActionResult<PingLogs> GetLogs()
        {
            IReadOnlyList<string> logs = _loggerBuffer.GetFirst();

            var response = new PingLogs
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                Count = logs.Count,
                Messages = logs
                    .Take(100)
                    .ToList(),
            };

            return Ok(response);
        }

        /// <summary>
        /// Ping to get current state of the ML Service
        ///
        /// Booting - System is booting
        /// Starting - Service is starting
        /// Running - Service is running
        /// Failed - Service has failed to start
        ///
        /// </summary>
        /// <returns>status</returns>
        [Description("Returns the state of the server")]
        [HttpGet]
        public ActionResult<PingResponse> Ping()
        {
            return Ok(GetOkResponse());
        }

        /// <summary>
        /// Returns 200 if the server is ready to process requests
        /// </summary>
        /// <returns></returns>
        [Description("Returns 200 if the server is ready to process requests")]
        [HttpGet("ready")]
        public ActionResult Ready()
        {
            return Ok(GetOkResponse());
        }

        /// <summary>
        /// Return 200 if the server is running
        /// </summary>
        /// <returns></returns>
        [Description("Returns 200 if the server is running")]
        [HttpGet("running")]
        public ActionResult Running()
        {
            return Ok(GetOkResponse());
        }

        private PingResponse GetOkResponse() => new PingResponse
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
            Status = "Running",
        };
    }
}
