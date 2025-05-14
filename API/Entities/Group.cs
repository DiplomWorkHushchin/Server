namespace API.Entities;

public class Group
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public ICollection<User> Students { get; set; } = new List<User>();
}
