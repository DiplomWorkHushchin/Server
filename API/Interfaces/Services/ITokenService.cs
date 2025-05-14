using API.Entities;
using System.Security.Claims;

namespace API.Interfaces;

public interface ITokenService
{
    Task<(string accesToken, RefreshToken refreshToken)> CreateToken(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<User> GetUserFromTokenAsync(string token);
}
