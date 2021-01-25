using Microsoft.AspNetCore.Mvc;
using Spin.Common.Model;
using Spin.Common.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Logging;
using Toolbox.Model;

namespace Spin.Common.Controller
{
    public class PingControllerBase : ControllerBase
    {
        private readonly IServiceStatus _serviceStatus;
        private readonly LoggerBuffer _loggerBuffer;

        public PingControllerBase(IServiceStatus serviceStatus, LoggerBuffer loggerBuffer)
        {
            _serviceStatus = serviceStatus;
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
        [Description("Returns 200 if the server is running")]
        [HttpGet]
        public ActionResult<PingResponse> Ping()
        {
            PingResponse pingResponse = GetResponse();

            return _serviceStatus.Level switch
            {
                ServiceStatusLevel.Running => Ok(pingResponse),
                ServiceStatusLevel.Ready => Ok(pingResponse),

                _ => StatusCode((int)HttpStatusCode.ServiceUnavailable, pingResponse),
            };
        }

        /// <summary>
        /// Returns 200 if the server is ready to process requests
        /// </summary>
        /// <returns></returns>
        [Description("Returns 200 if the server is ready to process requests, 503 if service is not ready")]
        [HttpGet("ready")]
        public ActionResult Ready()
        {
            PingResponse pingResponse = GetResponse();

            return _serviceStatus.Level switch
            {
                ServiceStatusLevel.Ready => Ok(pingResponse),

                _ => StatusCode((int)HttpStatusCode.ServiceUnavailable, pingResponse),
            };
        }

        private PingResponse GetResponse() => new PingResponse
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
            Status = _serviceStatus.Level.ToString(),
            Message = _serviceStatus.Message,
        };
    }
}
