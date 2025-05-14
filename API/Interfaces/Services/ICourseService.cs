using API.DTOs.CourseDTOs;
using API.DTOs.CourseDTOs.Tasks;
using API.DTOs.UserDTOs;
using API.Entities.Courses;

namespace API.Interfaces.Services;


public interface ICourseService
{
    Task<CourseDto> GetCourseByIdAsync(int courseId);
    Task<CourseDto> GetCourseByCodeAsync(string courseCode);
    Task<List<CourseDto>> GetCoursesAsync(HttpRequest request);

    Task<List<UserDto>> GetCourseTeachersByCodeAsync(string courseCode);
    Task<List<UserDto>> GetCourseStudentsByCodeAsync(string courseCode);


    Task CreateCourseAsync(CreateCourseDto createCourseDto, HttpRequest request);
    Task DeleteCourseAsync(string courseCode, HttpRequest request);

    Task AddUserAsync(CourseAddUserDto courseAddUserDto, HttpRequest request);
    Task EditCourseAsync(EditCourseDto editCourseDto, HttpRequest request);

    
}
