namespace API.DTOs.CourseDTOs.Tasks;

public class UpdateTaskDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Type { get; set; }
    public required DateTime DueDate { get; set; }
    public required string DueTime { get; set; }
    public required int MaxPoints { get; set; }

    public List<string>? UpdatedMaterials { get; set; }
    public List<IFormFile>? Materials { get; set; }
}

