namespace API.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = "";
    public DateTime ExpiryDate { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
