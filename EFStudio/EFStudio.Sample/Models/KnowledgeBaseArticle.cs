namespace EFStudio.Sample.Sqlite.Models;

public class KnowledgeBaseArticle
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public string? Summary { get; set; }
    public required string Body { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public int ViewCount { get; set; }
    public bool IsArchived { get; set; }
    public Company? Company { get; set; }
    public Employee? Author { get; set; }
}
