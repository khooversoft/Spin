using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.AspNetCore.Mvc;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DirectoryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SigningController : ControllerBase
{
    private readonly SigningService _signingService;

    public SigningController(SigningService identityService)
    {
        _signingService = identityService;
    }


    [HttpPost("sign")]
    public async Task<IActionResult> Sign([FromBody] SignRequest signRequest, CancellationToken token)
    {
        SignRequestResponse response = await _signingService.Sign(signRequest, token);
        if (response == null) return BadRequest();

        return Ok(response);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateRequest validateRequest, CancellationToken token)
    {
        bool result = await _signingService.Validate(validateRequest, token);
        if (result == false) return BadRequest();

        return Ok(true);
    }
}
