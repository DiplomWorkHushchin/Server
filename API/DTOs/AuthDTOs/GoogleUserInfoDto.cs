using System.Text.Json.Serialization;

namespace API.DTOs.AuthDTOs;

public class GoogleUserInfoDto
{
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("given_name")]
    public string? GoogleGivenName { get; set; }

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("email_verified")]
    public bool? EmailVerified { get; set; }
}
