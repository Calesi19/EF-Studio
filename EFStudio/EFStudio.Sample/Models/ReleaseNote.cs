namespace EFStudio.Sample.Sqlite.Models;

public class ReleaseNote
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Version { get; set; }
    public required string Title { get; set; }
    public required string Highlights { get; set; }
    public DateTime ReleasedAtUtc { get; set; }
    public ReleaseKind Kind { get; set; }
    public bool IsBreakingChange { get; set; }
    public Project? Project { get; set; }
    public Employee? Author { get; set; }
}
