using API.DTOs.UserDTOs;
using API.Entities;

namespace API.DTOs.CourseDTOs.Tasks;

public class SubmissionDto
{
    public int Id { get; set; }
    public int? Points { get; set; }

    public List<SubmissionFileDto> SubmissionFiles { get; set; } = new List<SubmissionFileDto>();

    public UserDto? User { get; set; }

}
