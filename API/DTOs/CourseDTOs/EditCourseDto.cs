namespace API.DTOs.CourseDTOs;

public class EditCourseDto
{
    public required string CourseCode { get; set; }
    public required string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public required string Category { get; set; } = default!;
    public required float Credits { get; set; } = default!;

    public IFormFile? CoverBanner { get; set; }

    public string Status { get; set; } = "upcoming";

    public List<CourseScheduleDto> Schedule { get; set; } = new();
}
