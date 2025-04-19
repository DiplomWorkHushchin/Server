using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services;

public class TokenService(IConfiguration config, UserManager<User> userManager, DataContext context) : ITokenService
{
    public async Task<(string accesToken, RefreshToken refreshToken)> CreateToken(User user)
    {
        var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token key from config");
        if (tokenKey.Length < 64) throw new Exception("Token key is too short, must be at least 64 characters");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        if (user.UserName == null) throw new Exception("User name is null");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserName)
        };

        var roles = await userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = creds
        };  

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.Id
        };

        var oldTokens = await context.RefreshToken.Where(t => t.UserId == user.Id).ToListAsync();
        context.RefreshToken.RemoveRange(oldTokens);

        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);

        return (accessToken, refreshToken);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
