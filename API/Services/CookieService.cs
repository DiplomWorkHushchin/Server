using API.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace API.Services;

public class CookieService(
    IHttpContextAccessor httpContextAccessor
    ) : ICookieService
{
    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (httpContextAccessor.HttpContext?.Response != null)
            httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        else
            throw new InvalidOperationException("HttpContext or Response is null");
    }
}
