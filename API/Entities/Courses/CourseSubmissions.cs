namespace API.Entities.Courses;

public class Submissions
{
    public int Id { get; set; }
    public int? Points { get; set; }

    public List<SubmissionsFiles> SubmissionsFiles { get; set; } = new List<SubmissionsFiles>();

    public required DateTime Created { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int TaskId { get; set; }
    public CourseMaterial Task { get; set; } = default!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;
}
