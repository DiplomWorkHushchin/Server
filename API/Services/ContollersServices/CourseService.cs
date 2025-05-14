using API.Data;
using API.DTOs.CourseDTOs;
using API.DTOs.UserDTOs;
using API.Entities.Courses;
using API.Exceptions;
using API.Interfaces;
using API.Interfaces.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class CourseService(
    ITokenService tokenService,
    DataContext context,
    IMapper mapper,
    IFileService fileService,
    ICoursePermissionsService coursePermissionsService
    ) : ICourseService
{
    // CREATE a course
    public async Task CreateCourseAsync(CreateCourseDto createCourseDTO, HttpRequest request)
    {
        var courseCreatorToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (courseCreatorToken == null) throw new UnauthorizedAccessException();

        var courseCreator = await tokenService.GetUserFromTokenAsync(courseCreatorToken);
        if (courseCreator == null) throw new UnauthorizedAccessException();

        string filePath = string.Empty;

        if (createCourseDTO.CoverBanner != null && createCourseDTO.CoverBanner.Length != 0)
            filePath = await fileService.SaveFileAsync(createCourseDTO.CoverBanner, "CoursesBanners");

        var course = new Course
        {
            Title = createCourseDTO.Title,
            Description = createCourseDTO.Description,
            CourseSchedule = createCourseDTO.Schedule.Select(s => new CourseSchedule
            {
                Day = s.Day,
                Time = s.Time,
                Location = s.Location
            }).ToList(),
            Created = DateTime.UtcNow,
            Code = string.Empty,
            Category = createCourseDTO.Category,
            Credits = createCourseDTO.Credits,
            CoverBanner = filePath,
            Status = Enum.Parse<CourseStatus>(createCourseDTO.Status, true),
        };

        if (!string.IsNullOrWhiteSpace(createCourseDTO.StartDate) &&
        DateTime.TryParse(createCourseDTO.StartDate, out var startDateParsed))
            course.StartDate = startDateParsed.ToUniversalTime();

        if (!string.IsNullOrWhiteSpace(createCourseDTO.EndDate) &&
            DateTime.TryParse(createCourseDTO.EndDate, out var endDateParsed))
            course.EndDate = endDateParsed.ToUniversalTime();

        course.Instructors.Add(new CourseInstructor
        {
            InstructorId = courseCreator.Id,
            Course = course,
            Owner = true,
            Permissions = new CoursePermission
            {
                CanCreateAssignments = true,
                CanModifyAssignments = true,
                CanGradeStudents = true,
                CanManageUsers = true
            }
        });

        var instructorEmails = createCourseDTO.Instructors
            .Select(i => i.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct()
            .ToList();

        if (instructorEmails.Any())
        {
            var instructorUsers = await context.Users
                .Where(u => instructorEmails.Contains(u.Email))
                .ToListAsync();

            foreach (var instructor in instructorUsers)
            {
                var instructorFromDto = createCourseDTO.Instructors.FirstOrDefault(i => i.Email == instructor.Email);
                if (instructorFromDto == null)
                    throw new ArgumentException("User with email not found", instructor.Email);

                course.Instructors.Add(new CourseInstructor
                {
                    InstructorId = instructor.Id,
                    Course = course,
                    Owner = false,
                    Permissions = new CoursePermission
                    {
                        CanCreateAssignments = instructorFromDto.CanCreateAssignments,
                        CanModifyAssignments = instructorFromDto.CanModifyAssignments,
                        CanGradeStudents = instructorFromDto.CanGradeStudents,
                        CanManageUsers = instructorFromDto.CanManageUsers
                    }
                });
            }
        }

        try
        {
            context.Courses.Add(course);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("Error saving course to the database", ex);
        }


        string abbreviation = new string(course.Category
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]))
            .ToArray());

        string generatedCode = $"{abbreviation}{course.Id}";

        course.Code = generatedCode;
        await context.SaveChangesAsync();
    }

    // DELETE course
    public async Task DeleteCourseAsync(string courseCode, HttpRequest request)
    {
        string? userToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (userToken == null) throw new ForbidException();

        var user = await tokenService.GetUserFromTokenAsync(userToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseCode, user.Id))
            throw new ForbidException("You don`t have permissions to delete this course");

        var course = await context.Courses
            .Include(c => c.CourseSchedule)
            .Include(c => c.Instructors)
            .Include(c => c.EnrolledStudents)
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == courseCode.ToUpper());

        if (course == null)
            throw new KeyNotFoundException("Course not exist");

        if (!string.IsNullOrEmpty(course.CoverBanner))
            fileService.DeleteFile(course.CoverBanner);

        context.Courses.Remove(course);

        await context.SaveChangesAsync();
    }

    // GET course by ID
    public async Task<CourseDto> GetCourseByIdAsync(int courseId)
    {
        var course = await context.Courses
            .Include(c => c.CourseSchedule)
            .Include(c => c.Instructors)
                .ThenInclude(ci => ci.Instructor)
            .Include(c => c.EnrolledStudents)
                .ThenInclude(cs => cs.Student)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            throw new KeyNotFoundException("Course not founded");

        return mapper.Map<CourseDto>(course);
    }

    // GET course by code
    public async Task<CourseDto> GetCourseByCodeAsync(string courseCode)
    {
        var course = await context.Courses
            .Include(c => c.CourseSchedule)
            .Include(c => c.Instructors)
                .ThenInclude(ci => ci.Instructor)
                .ThenInclude(u => u.Photos)
            .Include(c => c.EnrolledStudents)
                .ThenInclude(cs => cs.Student)
                .ThenInclude(u => u.Photos)
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == courseCode.ToUpper());

        if (course == null)
            throw new KeyNotFoundException("Course not founded");

        return mapper.Map<CourseDto>(course);
    }

    // GET ALL courses
    public async Task<List<CourseDto>> GetCoursesAsync(HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        var courses = await context.Courses
            .Include(c => c.CourseSchedule)
            .Include(c => c.Instructors)
                .ThenInclude(ci => ci.Instructor)
                .ThenInclude(u => u.Photos)
            .Include(c => c.EnrolledStudents)
                .ThenInclude(cs => cs.Student)
                .ThenInclude(u => u.Photos)
            .Where(c => c.Instructors.Any(ci => ci.InstructorId == accessorUser.Id) ||
                        c.EnrolledStudents.Any(cs => cs.StudentId == accessorUser.Id))
            .ToListAsync();

        if (courses == null || !courses.Any())
            throw new KeyNotFoundException("Courses not founded");

        return mapper.Map<List<CourseDto>>(courses);
    }

    // GET course teachers by code
    public async Task<List<UserDto>> GetCourseTeachersByCodeAsync(string courseCode)
    {
        var instructorIds = await context.Courses
            .Where(c => c.Code == courseCode)
            .SelectMany(c => c.Instructors.Select(ci => ci.InstructorId))
            .ToListAsync();

        List<UserDto> instructorsDto = [];

        if (instructorIds != null || instructorIds.Any())
        {
            var instructors = await context.Users
            .Where(u => instructorIds.Contains(u.Id))
            .Include(u => u.Photos)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();

            instructorsDto = mapper.Map<List<UserDto>>(instructors);
        }

        return instructorsDto;

    }

    // GET course students by code
    public async Task<List<UserDto>> GetCourseStudentsByCodeAsync(string courseCode)
    {
        var studentIds = await context.Courses
            .Where(c => c.Code == courseCode)
            .SelectMany(c => c.EnrolledStudents.Select(ci => ci.StudentId))
            .ToListAsync();

        List<UserDto> instructorsDto = [];

        if (studentIds != null || studentIds.Any())
        {
            var instructors = await context.Users
                .Where(u => studentIds.Contains(u.Id))
                .Include(u => u.Photos)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();

            instructorsDto = mapper.Map<List<UserDto>>(instructors);
        }
        
        return instructorsDto;
    }

    // PUT add user to course
    public async Task AddUserAsync(CourseAddUserDto courseAddUserDto, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseAddUserDto.CourseCode, accessorUser.Id, CourseAccessLevel.CanManageUsers))
            throw new ForbidException("You don`t have permissions to delete this course");

        var user = await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == courseAddUserDto.UserEmail.ToUpper());

        if (user == null)
            throw new KeyNotFoundException("User not founded");

        var course = await context.Courses.FirstOrDefaultAsync(context => context.Code.ToUpper() == courseAddUserDto.CourseCode.ToUpper());

        if (course == null)
            throw new KeyNotFoundException("Course not founded");

        if (course.EnrolledStudents.Any(cs => cs.StudentId == user.Id))
            throw new ArgumentException("User already enrolled in this course");

        if (course.Instructors.Any(ci => ci.InstructorId == user.Id))
            throw new ArgumentException("User already enrolled in this course");

        if (user.UserRoles.Any(ur => ur.Role.Name == "Student"))
        {
            course.EnrolledStudents.Add(new CourseStudent
            {
                StudentId = user.Id,
                CourseId = course.Id,
                Progress = 0,
            });
        }
        else if (user.UserRoles.Any(ur => ur.Role.Name == "Teacher"))
        {
            course.Instructors.Add(new CourseInstructor
            {
                InstructorId = user.Id,
                CourseId = course.Id,
                Owner = false,
                Permissions = new CoursePermission
                {
                    CanCreateAssignments = false,
                    CanModifyAssignments = false,
                    CanGradeStudents = false,
                    CanManageUsers = false
                }
            });
        }
        else
        {
            throw new ArgumentException("User role not supported");
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("Error saving course to the database", ex);
        }
    }

    // PUT edit course data
    public async Task EditCourseAsync(EditCourseDto editCourseDto, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(editCourseDto.CourseCode, accessorUser.Id))
            throw new ForbidException("You don`t have permissions to edit this course");

        var course = await context.Courses
            .Include(c => c.CourseSchedule)
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == editCourseDto.CourseCode.ToUpper());

        if (course == null)
            throw new KeyNotFoundException("Course not founded");

        string filePath = string.Empty;

        if (editCourseDto.CoverBanner != null && editCourseDto.CoverBanner.Length != 0)
        {
            if (!string.IsNullOrWhiteSpace(course.CoverBanner))
            {
                fileService.DeleteFile(course.CoverBanner);
                course.CoverBanner = null;
            }
            
            filePath = await fileService.SaveFileAsync(editCourseDto.CoverBanner, "CoursesBanners");
            course.CoverBanner = filePath;
        }

        course.Title = editCourseDto.Title;
        course.Description = editCourseDto.Description;
        course.Category = editCourseDto.Category;
        course.Credits = editCourseDto.Credits;

        if (!string.IsNullOrWhiteSpace(editCourseDto.StartDate) &&
        DateTime.TryParse(editCourseDto.StartDate, out var startDateParsed))
            course.StartDate = startDateParsed.ToUniversalTime();

        if (!string.IsNullOrWhiteSpace(editCourseDto.EndDate) &&
            DateTime.TryParse(editCourseDto.EndDate, out var endDateParsed))
            course.EndDate = endDateParsed.ToUniversalTime();

        course.Status = editCourseDto.Status.ToLower() switch
        {
            "in-progress" => CourseStatus.InProgress,
            "upcoming" => CourseStatus.Upcoming,
            "completed" => CourseStatus.Completed,
            _ => throw new ArgumentException("Invalid course status value")
        };

        context.CourseSchedules.RemoveRange(course.CourseSchedule);

        course.CourseSchedule = editCourseDto.Schedule.Select(s => new CourseSchedule
        {
            Day = s.Day,
            Time = s.Time,
            Location = s.Location
        }).ToList();

        await context.SaveChangesAsync();
    }

    
}
