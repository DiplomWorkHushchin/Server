namespace API.DTOs.CourseDTOs;

public class CourseInstructorDto
{
    public required string UserName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? FatherName { get; set; }
    public required string Email { get; set; }
    public string? PhotoUrl { get; set; }

    public bool? Owner { get; set; }

    public bool CanCreateAssignments { get; set; }
    public bool CanModifyAssignments { get; set; }
    public bool CanGradeStudents { get; set; }
    public bool CanManageUsers { get; set; }
}
