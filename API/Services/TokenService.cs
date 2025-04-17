using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(User user)
    {
        // Get token from appsettings if not present rise exception
        var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token key from config");

        if (tokenKey.Length < 64) throw new Exception("Token key is too short, must be at least 64 characters");

        // Create a symmetric security key using the token key from appsettings
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        // Create claims for the token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserName)
        };

        // Create a claims identity using the claims
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // Create a token descriptor using the claims identity and signing credentials
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };  

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
