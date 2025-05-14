using API.DTOs.CourseDTOs;
using API.DTOs.CourseDTOs.Tasks;
using API.DTOs.UserDTOs;
using API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.CourseControllers;

public class CoursesController(
    ICourseService courseService,
    ITaskService taskService
    ) : BaseApiController
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await courseService.GetCoursesAsync(Request);
        return Ok(courses);
    }

    [Authorize]
    [HttpGet]
    [Route("{courseId}")]
    public async Task<IActionResult> GetCourseById(int courseId)
    {
        var course = await courseService.GetCourseByIdAsync(courseId);
        return Ok(course);
    }

    [Authorize]
    [HttpGet]
    [Route("code/{courseCode}")]
    public async Task<ActionResult<CourseDto>> GetCourseByCode(string courseCode)
    {
        var course = await courseService.GetCourseByCodeAsync(courseCode);
        return Ok(course);
    }

    [Authorize]
    [HttpGet]
    [Route("teachers/{courseCode}")]
    public async Task<ActionResult<List<UserDto>>> GetCourseTeachersByCode(string courseCode)
    {
        var teachers = await courseService.GetCourseTeachersByCodeAsync(courseCode);
        return Ok(teachers);
    }

    [Authorize]
    [HttpGet]
    [Route("students/{courseCode}")]
    public async Task<ActionResult<List<UserDto>>> GetCourseStudentsByCode(string courseCode)
    {
        var students = await courseService.GetCourseStudentsByCodeAsync(courseCode);
        return Ok(students);
    }


    [Authorize(Roles = "Teacher,Admin")]
    [HttpDelete]
    [Route("{courseCode}")]
    public async Task<IActionResult> DeleteCourse(string courseCode)
    {
        await courseService.DeleteCourseAsync(courseCode, Request);
        return NoContent();
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateCourse(CreateCourseDto createCourseDto)
    {
        await courseService.CreateCourseAsync(createCourseDto, Request);
        return Created();
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut]
    [Route("add-user")]
    public async Task<IActionResult> AddUser(CourseAddUserDto courseAddUserDto)
    {
        await courseService.AddUserAsync(courseAddUserDto, Request);
        return NoContent();
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut]
    [Route("edit")]
    public async Task<IActionResult> EditCourse(EditCourseDto editCourseDto)
    {
        await courseService.EditCourseAsync(editCourseDto, Request);
        return NoContent();
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [Route("{courseCode}/tasks/create")]
    public async Task<IActionResult> CreateTask(CreateMaterialDto createMaterialDto, string courseCode)
    {
        await taskService.CreateTaskAsync(createMaterialDto, courseCode, Request);
        return Created();
    }

    [Authorize]
    [HttpGet]
    [Route("{courseCode}/tasks")]
    public async Task<ActionResult<List<CourseMaterialDto>>> GetTasks(string courseCode)
    {
        var tasks = await taskService.GetTasksAsync(courseCode);
        return Ok(tasks);
    }

    [Authorize]
    [HttpGet]
    [Route("{courseCode}/tasks/{taskId}")]
    public async Task<ActionResult<CourseMaterialDto>> GetTaskById(string courseCode, int taskId)
    {
        var task = await taskService.GetTasksByIdAsync(courseCode, taskId);
        return Ok(task);
    }

    // DELETE task
    [Authorize(Roles = "Teacher,Admin")]
    [HttpDelete]
    [Route("{courseCode}/tasks/{taskId}")]
    public async Task<IActionResult> DeleteTask(string courseCode, int taskId)
    {
        await taskService.DeleteTaskAsync(courseCode, taskId, Request);
        return NoContent();
    }

    //PUT edit task
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut]
    [Route("{courseCode}/tasks/{taskId}")]
    public async Task<IActionResult> EditTask(string courseCode, int taskId, UpdateTaskDto updateTaskDto)
    {
        await taskService.UpdateTaskAsync(courseCode, taskId, updateTaskDto, Request);
        return NoContent();
    }

    // POST sumbit task
    [Authorize(Roles = "Student")]
    [HttpPost]
    [Route("{courseCode}/tasks/{taskId}/submit")]
    public async Task<IActionResult> SubmitTask(string courseCode, int taskId, SubmitTaskDto submitTaskDto)
    {
        await taskService.SubmitTaskAsync(courseCode, taskId, submitTaskDto, Request);
        return Created();
    }

    //DELETE task submission
    [Authorize(Roles = "Student")]
    [HttpDelete]
    [Route("{courseCode}/tasks/{taskId}/submission")]
    public async Task<IActionResult> DeleteTaskSubmission(string courseCode, int taskId)
    {
        await taskService.DeleteSubmissionTaskAsync(courseCode, taskId, Request);
        return NoContent();
    }

    // GET task submission by user
    [Authorize(Roles = "Student,Admin,Teacher")]
    [HttpGet]
    [Route("{courseCode}/tasks/{taskId}/submission")]
    public async Task<IActionResult> GetSubmission(string courseCode, int taskId)
    {
        var submission = await taskService.GetSubmissionTaskAsync(courseCode, taskId, Request);
        return Ok(submission);
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet]
    [Route("{courseCode}/tasks/{taskId}/submission/review")]
    public async Task<ActionResult<List<SubmissionDto>>> GetSubmissionReview(string courseCode, int taskId)
    {
        var submission = await taskService.GetSubmissionsForTaskAsync(courseCode, taskId, Request);
        return Ok(submission);
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpPut]
    [Route("{courseCode}/tasks/{taskId}/submission/review")]
    public async Task<IActionResult> ReviewSubmission(string courseCode, int taskId, List<ReviewSubmissionDto> reviewSubmissionDto)
    {
        await taskService.ReviewSubmissionAsync(courseCode, taskId, reviewSubmissionDto, Request);
        return NoContent();
    }
}
