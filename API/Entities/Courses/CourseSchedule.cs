namespace API.Entities.Courses;

public class CourseSchedule
{
    public int Id { get; set; }

    public string Day { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public int CourseId { get; set; }
    public Course? Course { get; set; }
}
