using API.Entities.Courses;

namespace API.DTOs.CourseDTOs;

public class CourseDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? CoverBanner { get; set; }
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public required string Code { get; set; }
    public required string Category { get; set; }
    public required float Credits { get; set; }
    //public required int Progress { get; set; }

    public List<CourseScheduleDto> CourseSchedule { get; set; } = new();
    public List<CourseInstructorDto> Instructors { get; set; } = new();
    public List<CourseStudentDto> EnrolledStudents { get; set; } = new();


    public CourseStatus Status { get; set; }
}
