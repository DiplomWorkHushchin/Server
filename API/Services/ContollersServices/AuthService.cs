using API.Data;
using API.DTOs.AuthDTOs;
using API.DTOs.UserDTOs;
using API.Entities;
using API.Interfaces;
using API.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace API.Services.ContollersServices;

public class AuthService(
    UserManager<User> userManager,
    ITokenService tokenService,
    IMapper mapper,
    DataContext context
    ) : IAuthService
{
    public async Task<UserAuthDto> RegisterUserAsync(RegisterDto registerDto, HttpResponse response)
    {
        var username = registerDto.Username.ToLower();

        if (await context.Users.AnyAsync(u => u.NormalizedUserName == username.ToUpper()))
            throw new ArgumentException("User with name already exist");

        var group = await context.Groups.FirstOrDefaultAsync(g => g.Name == registerDto.GroupName);
        if (group == null)
        {
            group = new Group { Name = registerDto.GroupName };
            context.Groups.Add(group);
            await context.SaveChangesAsync();
        }

        var user = new User
        {
            UserName = username,
            Email = registerDto.Email,
            FatherName = registerDto.FatherName ?? "",
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = "",
            GroupId = group.Id,
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role { Name = registerDto.Role }
                }
            },
            Photos = new List<UserPhoto>()
        };

        var result = await userManager.CreateAsync(user, "Pa$$w0rd");
        if (!result.Succeeded)
            throw new ArgumentException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        });

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = accessToken
        };
    }

    public async Task<UserAuthDto> LoginUserAsync(LoginDto loginDto, HttpResponse response)
    {
        var normalizedEmail = loginDto.Email.ToUpper();

        var user = await userManager.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

        if (user == null || user.Email == null)
            throw new UnauthorizedAccessException("Invalid email");

        var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Check your login data");

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = accessToken,
        };
    }

    public async Task LogoutUserAsync(HttpRequest request, HttpResponse response)
    {
        var refreshToken = request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var refreshInDb = await context.RefreshToken
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (refreshInDb != null)
        {
            context.RefreshToken.Remove(refreshInDb);
            await context.SaveChangesAsync();
        }

        response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1)
        });
    }

    public async Task<UserAuthDto> LoginUserViaGoogleAsync(HttpRequest request, HttpResponse response)
    {
        var temporaryAccessToken = request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(temporaryAccessToken))
            throw new UnauthorizedAccessException("Cannot find tokens: access");

        var user = await tokenService.GetUserFromTokenAsync(temporaryAccessToken);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = accessToken,
        };
    }

    public async Task<IActionResult> GoogleCallbackAsync(HttpContext httpContext)
    {
        var authenticateResult = await httpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            return new BadRequestObjectResult("Error in authentication");
        }

        var token = authenticateResult.Properties.Items[".Token.access_token"];
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        if (!response.IsSuccessStatusCode)
            return new BadRequestObjectResult("Failed to fetch user info");

        var json = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfoDto>(json);

        if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            return new BadRequestObjectResult("Invalid Google user information");

        var user = await userManager.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == userInfo.Email.ToUpper());
        if (user == null || user.Email == null)
            return new RedirectResult("https://localhost:3000/login?error=InvalidUser");

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        var clientUrl = "https://localhost:3000/login?token=" + accessToken;
        return new RedirectResult(clientUrl);
    }

    public async Task<UserAuthDto> UpdateUserTokensAsync(HttpResponse response, HttpRequest request)
    {
        var refreshToken = request.Cookies["refreshToken"];
        var accessToken = request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(accessToken))
            throw new UnauthorizedAccessException("Cannot find tokens: access");

        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Cannot find tokens: refresh");

        var user = await tokenService.GetUserFromTokenAsync(accessToken);
        if (user == null)
            throw new UnauthorizedAccessException("No users found");

        var oldRefreshToken = user.RefreshTokens.FirstOrDefault();
        if (oldRefreshToken == null || oldRefreshToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedAccessException("No valid refresh token found");

        // Генерируем новые токены
        var (newAccessToken, newRefreshToken) = await tokenService.CreateToken(user);

        // Устанавливаем новый refreshToken в куки
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = newAccessToken
        };
    }
}
