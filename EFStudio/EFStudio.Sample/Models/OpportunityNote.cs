namespace EFStudio.Sample.Sqlite.Models;

public class OpportunityNote
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsPinned { get; set; }
    public SalesOpportunity? Opportunity { get; set; }
    public Employee? Author { get; set; }
}
