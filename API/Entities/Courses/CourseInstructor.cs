namespace API.Entities.Courses;

public class CourseInstructor
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public int InstructorId { get; set; }
    public User Instructor { get; set; } = default!;

    public required bool Owner { get; set; } = false;
    public required CoursePermission Permissions { get; set; }
}
