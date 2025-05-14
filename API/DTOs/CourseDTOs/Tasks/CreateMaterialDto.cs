using API.Entities.Courses;

namespace API.DTOs.CourseDTOs.Tasks;

public class CreateMaterialDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string MaterialType { get; set; }
    public string? MaxPoints { get; set; } = null;

    public required DateTime DueDate { get; set; }
    public required string DueTime { get; set; }

    public List<IFormFile> MaterialsFiles { get; set; } = new List<IFormFile>();
}
