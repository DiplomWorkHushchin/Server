namespace API.DTOs.CourseDTOs.Tasks;

public class MaterialFilesDto
{
    public required string Name { get; set; }
    public required IFormFile File{ get; set; }
}
