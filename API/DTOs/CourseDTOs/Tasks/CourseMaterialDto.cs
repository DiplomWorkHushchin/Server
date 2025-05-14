using API.Entities.Courses;

namespace API.DTOs.CourseDTOs.Tasks;

public class CourseMaterialDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public MaterialType MaterialType { get; set; } = MaterialType.Lecture;
    public int? MaxPoints { get; set; } = null;

    public DateTime DueDate { get; set; } = DateTime.Now;

    public List<CourseMaterialFilesDto> MaterialsFiles { get; set; } = new List<CourseMaterialFilesDto>();
}
