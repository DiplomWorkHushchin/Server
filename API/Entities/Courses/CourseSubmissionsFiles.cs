namespace API.Entities.Courses;

public class SubmissionsFiles
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string FilePath { get; set; }

    public int SubmissionId { get; set; }
    public Submissions Submissions{ get; set; } = default!;
}