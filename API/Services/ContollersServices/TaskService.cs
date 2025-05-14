using API.Data;
using API.DTOs.CourseDTOs.Tasks;
using API.Entities.Courses;
using API.Exceptions;
using API.Interfaces;
using API.Interfaces.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services.ContollersServices;

public class TaskService(
    DataContext context,
    IMapper mapper,
    ITokenService tokenService,
    ICoursePermissionsService coursePermissionsService,
    IFileService fileService
    ) : ITaskService
{
    // POST crate task
    public async Task CreateTaskAsync(CreateMaterialDto createMaterialDto, string courseCode, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseCode, accessorUser.Id, CourseAccessLevel.CanManageUsers))
            throw new ForbidException("You don`t have permissions to create tasks");

        var course = await context.Courses.FirstOrDefaultAsync(context => context.Code.ToUpper() == courseCode.ToUpper());

        if (course == null)
            throw new KeyNotFoundException("Course not founded");

        var materialType = (createMaterialDto.MaterialType ?? "").Trim().ToLower() switch
        {
            "lecture" => MaterialType.Lecture,
            "task" => MaterialType.Task,
            _ => MaterialType.Lecture
        };

        var dueDate = createMaterialDto.DueDate;
        var parsed = TimeSpan.TryParse(createMaterialDto.DueTime, out var dueTime);

        DateTime dueDateTime = DateTime.UtcNow;

        if (parsed)
        {
            var localDue = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, dueTime.Hours, dueTime.Minutes, 0, DateTimeKind.Unspecified);
            dueDateTime = TimeZoneInfo.ConvertTimeToUtc(localDue);
        }
        else
        {
            var localFallback = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 23, 59, 0, DateTimeKind.Unspecified);
            dueDateTime = TimeZoneInfo.ConvertTimeToUtc(localFallback);
        }


        var material = new CourseMaterial

        {
            Title = createMaterialDto.Title,
            Description = createMaterialDto.Description,
            MaterialType = materialType,
            Created = DateTime.UtcNow,
            DueDate = dueDateTime,
            CourseId = course.Id,
            MaterialsFiles = new List<CourseMaterialsFiles>()
        };

        if (!string.IsNullOrWhiteSpace(createMaterialDto.MaxPoints) && int.TryParse(createMaterialDto.MaxPoints, out int maxPoints))
        {
            material.MaxPoints = maxPoints;
        }


        foreach (var file in createMaterialDto.MaterialsFiles)
        {
            if (file.Length > 0)
            {

                var savedPath = await fileService.SaveFileAsync(file, "CourseMaterials");

                material.MaterialsFiles.Add(new CourseMaterialsFiles
                {
                    Name = Path.GetFileName(file.FileName),
                    FilePath = savedPath
                });
            }
        }

        course.Materials.Add(material);
        await context.SaveChangesAsync();
    }

    // GET all tasks
    public async Task<List<CourseMaterialDto>> GetTasksAsync(string courseCode)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == courseCode.ToUpper());

        if (course == null)
            throw new KeyNotFoundException("Course not found");

        var materials = await context.CourseMaterials
            .Where(m => m.CourseId == course.Id)
            .Include(m => m.MaterialsFiles)
            .ToListAsync();

        if (materials == null || !materials.Any())
            return new List<CourseMaterialDto>();

        var materialsDto = mapper.Map<List<CourseMaterialDto>>(materials);

        return materialsDto;
    }

    //Get task by ID
    public async Task<CourseMaterialDto> GetTasksByIdAsync(string courseCode, int taskId)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == courseCode.ToUpper());


        var task = await context.CourseMaterials
            .Include(m => m.MaterialsFiles)
            .FirstOrDefaultAsync(m => m.Id == taskId);

        var materialDto = mapper.Map<CourseMaterialDto>(task);

        return materialDto;
    }

    // DELETE task
    public async Task DeleteTaskAsync(string courseCode, int taskId, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseCode, accessorUser.Id))
            throw new ForbidException("You don`t have permissions to create tasks");

        var task = await context.CourseMaterials
            .Include(m => m.MaterialsFiles)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Course.Code == courseCode);

        if (task == null)
            throw new KeyNotFoundException("Task not found.");


        if (task.MaterialsFiles != null && task.MaterialsFiles.Any())
        {
            foreach (var file in task.MaterialsFiles)
            {
                if (!string.IsNullOrWhiteSpace(file.FilePath))
                {
                    fileService.DeleteFile(file.FilePath);
                }
            }

        }

        context.CourseMaterials.Remove(task);
        await context.SaveChangesAsync();
    }

    public async Task UpdateTaskAsync(string courseCode, int taskId, UpdateTaskDto editMaterialDto, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken))
            throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseCode, accessorUser.Id, CourseAccessLevel.CanModifyAssignments))
            throw new ForbidException("You don’t have permissions to modify tasks");

        var task = await context.CourseMaterials
            .Include(m => m.MaterialsFiles)
            .Include(m => m.Course)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Course.Code == courseCode);

        if (task == null)
            throw new KeyNotFoundException("Task not found.");

        var dueDate = editMaterialDto.DueDate;
        var parsed = TimeSpan.TryParse(editMaterialDto.DueTime, out var dueTime);

        DateTime dueDateTime = DateTime.UtcNow;

        if (parsed)
        {
            var localDue = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, dueTime.Hours, dueTime.Minutes, 0, DateTimeKind.Unspecified);
            dueDateTime = TimeZoneInfo.ConvertTimeToUtc(localDue);
        }
        else
        {
            var localFallback = new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 23, 59, 0, DateTimeKind.Unspecified);
            dueDateTime = TimeZoneInfo.ConvertTimeToUtc(localFallback);
        }

        task.Title = editMaterialDto.Title;
        task.Description = editMaterialDto.Description;
        task.MaterialType = (editMaterialDto.Type ?? "").Trim().ToLower() switch
        {
            "lecture" => MaterialType.Lecture,
            "task" => MaterialType.Task,
            _ => MaterialType.Lecture
        };
        task.DueDate = dueDateTime;
        task.MaxPoints = editMaterialDto.MaxPoints;


        foreach (var file in task.MaterialsFiles.ToList())
        {
            if (editMaterialDto.UpdatedMaterials == null || editMaterialDto.UpdatedMaterials.Count == 0)
            {
                fileService.DeleteFile(file.FilePath);
                context.CourseMaterialsFiles.Remove(file);
                continue;
            }

            bool existsInDto = editMaterialDto.UpdatedMaterials
                .Any(path => path == file.FilePath);

            if (!existsInDto)
            {
                fileService.DeleteFile(file.FilePath);
                context.CourseMaterialsFiles.Remove(file);
            }
        }


        if (editMaterialDto.Materials != null)
        {
            foreach (var file in editMaterialDto.Materials)
            {
                if (file != null && file.Length > 0)
                {
                    var savedPath = await fileService.SaveFileAsync(file, "CourseMaterials");

                    task.MaterialsFiles.Add(new CourseMaterialsFiles
                    {
                        Name = file.FileName,
                        FilePath = savedPath
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task SubmitTaskAsync(string courseCode, int taskId, SubmitTaskDto submitTaskDto, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken))
            throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        var course = await context.Courses
            .Include(c => c.Materials)
            .Include(c => c.EnrolledStudents)
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        if (course == null)
            throw new KeyNotFoundException($"Course '{courseCode}' not found");

        bool isEnrolled = course.EnrolledStudents.Any(es => es.StudentId == accessorUser.Id);
        if (!isEnrolled)
            throw new ForbidException("User not enrolled in the course");

        var task = course.Materials.FirstOrDefault(m => m.Id == taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with id {taskId} not found in course {courseCode}");

        var submission = new Submissions
        {
            UserId = accessorUser.Id,
            TaskId = task.Id,
            CourseId = course.Id,
            Created = DateTime.UtcNow,
        };

        if (submitTaskDto.Files != null && submitTaskDto.Files.Count > 0)
        {
            foreach (var file in submitTaskDto.Files)
            {
                if (file.Length > 0)
                {
                    var savedPath = await fileService.SaveFileAsync(file, "CourseSubmissions");
                    submission.SubmissionsFiles.Add(new SubmissionsFiles
                    {
                        Name = Path.GetFileName(file.FileName),
                        FilePath = savedPath
                    });
                }
            }
        }

        await context.Submissions.AddAsync(submission);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSubmissionTaskAsync(string courseCode, int taskId, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken))
            throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        var course = await context.Courses
            .Include(c => c.Materials)
            .Include(c => c.EnrolledStudents)
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        if (course == null)
            throw new KeyNotFoundException($"Course '{courseCode}' not found");

        bool isEnrolled = course.EnrolledStudents.Any(es => es.StudentId == accessorUser.Id);
        if (!isEnrolled)
            throw new ForbidException("User not enrolled in the course");

        var task = course.Materials.FirstOrDefault(m => m.Id == taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with id {taskId} not found in course {courseCode}");

        var submission = await context.Submissions
            .Include(s => s.SubmissionsFiles)
            .FirstOrDefaultAsync(s => s.TaskId == task.Id && s.UserId == accessorUser.Id);

        if (submission == null)
            throw new KeyNotFoundException($"Submission not found for task {taskId} in course {courseCode}");

        if (submission.SubmissionsFiles != null && submission.SubmissionsFiles.Any())
        {
            foreach (var file in submission.SubmissionsFiles)
            {
                if (!string.IsNullOrWhiteSpace(file.FilePath))
                {
                    fileService.DeleteFile(file.FilePath);
                }
            }
        }

        context.Submissions.Remove(submission);
        await context.SaveChangesAsync();
    }

    public async Task<SubmissionDto> GetSubmissionTaskAsync(string courseCode, int taskId, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken))
            throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        var course = await context.Courses
            .Include(c => c.Materials)
            .Include(c => c.EnrolledStudents)
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        if (course == null)
            throw new KeyNotFoundException($"Course '{courseCode}' not found");

        bool isEnrolled = course.EnrolledStudents.Any(es => es.StudentId == accessorUser.Id);
        if (!isEnrolled)
            throw new ForbidException("User not enrolled in the course");

        var task = course.Materials.FirstOrDefault(m => m.Id == taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with id {taskId} not found in course {courseCode}");

        var submission = await context.Submissions
            .Include(s => s.SubmissionsFiles)
            .FirstOrDefaultAsync(s => s.TaskId == task.Id && s.UserId == accessorUser.Id);

        var submissionDto = mapper.Map<SubmissionDto>(submission);

        return submissionDto;
    }

    public async Task<List<SubmissionDto>> GetSubmissionsForTaskAsync(string courseCode, int taskId, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseCode, accessorUser.Id, CourseAccessLevel.CanGradeStudents))
            throw new ForbidException("You don`t have permissions to grade tasks");

        var course = await context.Courses
            .Include(c => c.Instructors)
                .ThenInclude(cs => cs.Instructor)
                    .ThenInclude(s => s.Photos)
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        if (course == null)
            throw new KeyNotFoundException($"Course '{courseCode}' not found");

        bool isEnrolled = course.Instructors.Any(es => es.InstructorId == accessorUser.Id);
        if (!isEnrolled)
            throw new ForbidException("User not enrolled in the course");

        var submissions = await context.Submissions
           .Include(s => s.SubmissionsFiles)
           .Include(s => s.User)
               .ThenInclude(u => u.Photos)
           .Where(s => s.TaskId == taskId && s.Course.Code == courseCode)
           .ToListAsync();


        var submissionsDto = mapper.Map<List<SubmissionDto>>(submissions);

        return submissionsDto;
    }

    public async Task ReviewSubmissionAsync(string courseCode, int taskId, List<ReviewSubmissionDto> reviewSubmissionDto, HttpRequest request)
    {
        string? accessorUserToken = request.Headers["Authorization"].ToString().Split(" ").Last();
        if (string.IsNullOrWhiteSpace(accessorUserToken)) throw new ForbidException();

        var accessorUser = await tokenService.GetUserFromTokenAsync(accessorUserToken);

        if (!await coursePermissionsService.CanUserAccessCourseManage(courseCode, accessorUser.Id, CourseAccessLevel.CanGradeStudents))
            throw new ForbidException("You don`t have permissions to grade tasks");

        var course = await context.Courses
            .Include(c => c.Materials)
            .Include(c => c.Instructors)
                .ThenInclude(cs => cs.Instructor)
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        if (course == null)
            throw new KeyNotFoundException($"Course '{courseCode}' not found");

        bool isEnrolled = course.Instructors.Any(es => es.InstructorId == accessorUser.Id);
        if (!isEnrolled)
            throw new ForbidException("User not enrolled in the course");

        var task = course.Materials.FirstOrDefault(m => m.Id == taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task with id {taskId} not found in course {courseCode}");

        var submissions = await context.Submissions
            .Include(s => s.SubmissionsFiles)
            .Include(s => s.User)
                .ThenInclude(u => u.Photos)
            .Where(s => s.TaskId == task.Id && s.Course.Code == courseCode)
            .ToListAsync();

        if (submissions == null || !submissions.Any())
            throw new KeyNotFoundException($"Submissions not found for task {taskId} in course {courseCode}");

        foreach (var reviewSubmission in reviewSubmissionDto)
        {
            var submission = submissions.FirstOrDefault(s => s.Id == reviewSubmission.SubmissionId);
            if (submission == null)
                throw new KeyNotFoundException($"Submission with id {reviewSubmission.SubmissionId} not found");
            submission.Points = reviewSubmission.Points;
        }

        await context.SaveChangesAsync();
    }
}
