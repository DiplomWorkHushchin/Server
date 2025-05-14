using API.DTOs.CourseDTOs;
using API.DTOs.CourseDTOs.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces.Services;

public interface ITaskService
{
    Task CreateTaskAsync(CreateMaterialDto createMaterialDto, string courseCode, HttpRequest request);

    Task<List<CourseMaterialDto>> GetTasksAsync(string courseCode);
    Task<CourseMaterialDto> GetTasksByIdAsync(string courseCode, int taskId);

    Task DeleteTaskAsync(string courseCode, int taskId, HttpRequest request);
    Task UpdateTaskAsync(string courseCode, int taskId, UpdateTaskDto editMaterialDto, HttpRequest request);

    Task SubmitTaskAsync(string courseCode, int taskId, SubmitTaskDto submitTaskDto, HttpRequest request);
    Task DeleteSubmissionTaskAsync(string courseCode, int taskId, HttpRequest request);

    Task<SubmissionDto> GetSubmissionTaskAsync(string courseCode, int taskId, HttpRequest request);

    Task<List<SubmissionDto>> GetSubmissionsForTaskAsync(string courseCode, int taskId, HttpRequest request);

    Task ReviewSubmissionAsync(string courseCode, int taskId, List<ReviewSubmissionDto> reviewSubmissionDto, HttpRequest request);

}
