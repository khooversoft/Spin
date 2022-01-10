using Microsoft.AspNetCore.Mvc;
using Spin.Common.Controller;
using Spin.Common.Services;
using Toolbox.Logging;

namespace ContractApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : PingControllerBase
    {
        public PingController(IServiceStatus serviceStatus, ILoggerBuffer loggerBuffer)
            : base(serviceStatus, loggerBuffer)
        {
        }
    }
}