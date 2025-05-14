namespace API.Interfaces.Services;

public interface ICookieService
{
    void SetRefreshTokenCookie(string refreshToken);
}
