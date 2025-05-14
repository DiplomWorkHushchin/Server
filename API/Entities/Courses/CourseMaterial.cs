namespace API.Entities.Courses;

public enum MaterialType
{
    Lecture,
    Task
}

public class CourseMaterial
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public MaterialType MaterialType { get; set; } = MaterialType.Lecture;
    public int? MaxPoints { get; set; } = null;

    public required DateTime Created { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; } = DateTime.Now;

    public List<CourseMaterialsFiles> MaterialsFiles { get; set; } = new List<CourseMaterialsFiles>();

    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;
}
