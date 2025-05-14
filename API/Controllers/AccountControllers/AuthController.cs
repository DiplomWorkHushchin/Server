using API.DTOs.AuthDTOs;
using API.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AccountControllers;

public class AuthController(
    IAuthService authService
    ) : BaseApiController
{
    [Authorize(Roles = "Admin")]
    [HttpPost("register")] // /auth/register
    public async Task<ActionResult<UserAuthDto>> RegisterUser(RegisterDto registerDto)
    {
        var result = await authService.RegisterUserAsync(registerDto, Response);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")] // /auth/login
    public async Task<ActionResult<UserAuthDto>> LoginUser(LoginDto loginDto)
    {
        var result = await authService.LoginUserAsync(loginDto, Response);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("login-via-google")] // /auth/login-via-google
    public async Task<ActionResult<UserAuthDto>> LoginUserViaGoogle()
    {
        var result = await authService.LoginUserViaGoogleAsync(Request, Response);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet("google/callback")] // After authentication, Google redirects to this URL to provide the access token
    public async Task<IActionResult> GoogleCallback()
    {
        return await authService.GoogleCallbackAsync(HttpContext);
    }

    [Authorize]
    [HttpPost("logout")] // /auth/logout
    public async Task<ActionResult> LogoutUser()
    {
        await authService.LogoutUserAsync(Request, Response);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")] // /auth/refresh-token
    public async Task<ActionResult<UserAuthDto>> UpdateUserTokens()
    {
        var userAuthDto = await authService.UpdateUserTokensAsync(Response, Request);
        return Ok(userAuthDto);
    }
}

