using API.Entities.Courses;
using API.Entities;

public class CourseStudent
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public int StudentId { get; set; }
    public User Student { get; set; } = default!;

    public double? Progress { get; set; }
}