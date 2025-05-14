namespace API.Interfaces.Services;

public enum CourseAccessLevel
{
    Owner,
    CanCreateAssignments,
    CanModifyAssignments,
    CanGradeStudents,
    CanManageUsers
}

public interface ICoursePermissionsService
{
    Task<bool> CanUserAccessCourseManage(string courseCode, int userId);
    Task<bool> CanUserAccessCourseManage(string courseCode, int userId, CourseAccessLevel accessLevel);
}
