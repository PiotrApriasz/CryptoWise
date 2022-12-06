using CryptoWise.API.Authorization;
using CryptoWise.API.Extensions;
using CryptoWise.API.Services.Account;
using CryptoWise.Shared;
using CryptoWise.Shared.MetaAuthAccount;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWise.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MetaAuthController : BaseController
{
    private readonly IMetaAuthAccountService _metaAuthAccountService;

    public MetaAuthController(IMetaAuthAccountService metaAuthAccountService)
    {
        _metaAuthAccountService = metaAuthAccountService;
    }

    [AllowAnonymous]
    [HttpGet("initiateSignUp")]
    public async Task<IActionResult> InitiateSignUp()
    {
        var result = await _metaAuthAccountService.InitiateSignUp();
        result.CheckForNull();

        if (result.Error)
        {
            return BadRequest(new BaseResponse
            {
                Error = result.Error,
                Message = result.Message
            });
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("initiateSignIn")]
    public async Task<IActionResult> InitiateSignIn()
    {
        var result = await _metaAuthAccountService.InitiateSignIn();
        result.CheckForNull();

        if (result.Error)
        {
            return BadRequest(new BaseResponse()
            {
                Error = result.Error,
                Message = result.Message
            });
        }

        return Ok(result); 
    }

    [AllowAnonymous]
    [HttpGet("getAccessToken/{requestId}")]
    public async Task<IActionResult> GetAccessToken(string requestId)
    {
        var result = await _metaAuthAccountService.GetAccessToken(requestId);
        result.CheckForNull();

        if (result.Error)
        {
            return BadRequest(new BaseResponse
            {
                Error = true,
                Message = result.Message
            });
        }

        return Ok(result);
    }
}