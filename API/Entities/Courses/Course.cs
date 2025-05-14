using Microsoft.EntityFrameworkCore;

namespace API.Entities.Courses;

public enum CourseStatus
{
    InProgress,
    Upcoming,
    Completed
}

public class Course
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? CoverBanner { get; set; }
    public required string Code { get; set; }
    public required string Category { get; set; }
    public float? Credits { get; set; }

    public List<CourseSchedule> CourseSchedule { get; set; } = new();
    public List<CourseInstructor> Instructors { get; set; } = new List<CourseInstructor>();
    public List<CourseStudent> EnrolledStudents { get; set; } = new List<CourseStudent>();
    public List<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
    public List<Submissions> Submissions { get; set; } = new List<Submissions>();

    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

    public DateTime Created { get; set; } = DateTime.Now;
    public CourseStatus Status { get; set; } = CourseStatus.Upcoming;
}

