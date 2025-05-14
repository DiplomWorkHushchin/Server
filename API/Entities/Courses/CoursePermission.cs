namespace API.Entities.Courses;

public class CoursePermission
{
    public bool CanCreateAssignments { get; set; } = false;
    public bool CanModifyAssignments { get; set; } = false;
    public bool CanGradeStudents { get; set; } = false;
    public bool CanManageUsers { get; set; } = false;
}
