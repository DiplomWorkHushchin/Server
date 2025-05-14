using API.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces.Services;

public interface IAuthService
{
    Task<UserAuthDto> RegisterUserAsync(RegisterDto registerDto, HttpResponse response);
    Task<UserAuthDto> LoginUserAsync(LoginDto loginDto, HttpResponse response);
    Task LogoutUserAsync(HttpRequest request, HttpResponse response);

    Task<UserAuthDto> LoginUserViaGoogleAsync(HttpRequest request, HttpResponse response);
    Task<IActionResult> GoogleCallbackAsync(HttpContext httpContext);

    Task<UserAuthDto> UpdateUserTokensAsync(HttpResponse response, HttpRequest request);

}
