namespace EFStudio.Sample.Sqlite.Models;

public class Post
{
    public int Id { get; set; }
    public Guid AuthorId { get; set; }
    public int? CompanyId { get; set; }
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public int ViewCount { get; set; }
    public short AverageReadTimeMinutes { get; set; }
    public bool IsFeatured { get; set; }
    public Employee? Author { get; set; }
    public Company? Company { get; set; }
}
