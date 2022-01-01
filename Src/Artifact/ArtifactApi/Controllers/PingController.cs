using Microsoft.AspNetCore.Mvc;
using Spin.Common.Controller;
using Spin.Common.Services;
using Toolbox.Logging;

namespace Artifact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : PingControllerBase
    {
        public PingController(IServiceStatus serviceStatus, LoggerBuffer loggerBuffer)
            : base(serviceStatus, loggerBuffer)
        {
        }
    }
}