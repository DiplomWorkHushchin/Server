using System.Reflection.Metadata.Ecma335;

namespace API.Entities.Courses;

public class CourseMaterialsFiles
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string FilePath { get; set; }

    public int CourseMaterialId { get; set; }
    public CourseMaterial CourseMaterial { get; set; } = default!;
}
