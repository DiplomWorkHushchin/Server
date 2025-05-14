using API.Data;
using API.Entities.Courses;
using API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class CoursePermissionsService(
    DataContext context
    ) : ICoursePermissionsService
{
    public async Task<bool> CanUserAccessCourseManage(string courseCode, int userId)
    {
        return await CanUserAccessCourseManage(courseCode, userId, CourseAccessLevel.Owner);
    }

    public async Task<bool> CanUserAccessCourseManage(string courseCode, int userId, CourseAccessLevel accessLevel)
    {
        Course? course = await context.Courses.Where(c => c.Code.ToUpper() == courseCode.ToUpper())
            .Include(c => c.Instructors)
                .ThenInclude(ci => ci.Instructor)
            .FirstOrDefaultAsync();
        if (course == null)
            throw new KeyNotFoundException("Course with this instructor not found");


        CourseInstructor? instructor = course.Instructors.FirstOrDefault(i => i.InstructorId == userId);
        if ( instructor == null )
            throw new KeyNotFoundException("Instructor with this course not found");

        switch (accessLevel) {
            case CourseAccessLevel.Owner:
                if (instructor.Owner)
                    return true;
                break;

            case CourseAccessLevel.CanCreateAssignments:
                if (instructor.Permissions.CanCreateAssignments)
                    return true;
                break;

            case CourseAccessLevel.CanModifyAssignments:
                if (instructor.Permissions.CanModifyAssignments)
                   return true;
                break;

            case CourseAccessLevel.CanGradeStudents:
                if (instructor.Permissions.CanGradeStudents)
                    return true;
                break;

            case CourseAccessLevel.CanManageUsers:
                if (instructor.Permissions.CanManageUsers)
                    return true;
                break;

            default:
                return false;
        }

        return false;
    }
}
