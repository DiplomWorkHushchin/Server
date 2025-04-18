using API.Entities;
using System.Security.Claims;

namespace API.Interfaces;

public interface ITokenService
{
    Task<(string accesToken, string refreshToken)> CreateToken(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
