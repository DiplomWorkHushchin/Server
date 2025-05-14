using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class UserPhoto
{
    [Key]
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
