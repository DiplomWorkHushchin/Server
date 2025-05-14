namespace API.DTOs.CourseDTOs;

public class CourseStudentDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? FatherName { get; set; }
    public required string Email { get; set; }
    public string? PhotoUrl { get; set; }
}
