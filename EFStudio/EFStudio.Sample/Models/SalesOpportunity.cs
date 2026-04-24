namespace EFStudio.Sample.Sqlite.Models;

public class SalesOpportunity
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int CustomerId { get; set; }
    public Guid? OwnerEmployeeId { get; set; }
    public int? ProjectId { get; set; }
    public required string Name { get; set; }
    public decimal EstimatedValue { get; set; }
    public OpportunityStage Stage { get; set; }
    public DateOnly? ExpectedCloseOn { get; set; }
    public byte ConfidencePercent { get; set; }
    public string? Source { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Company? Company { get; set; }
    public Customer? Customer { get; set; }
    public Employee? OwnerEmployee { get; set; }
    public Project? Project { get; set; }
    public List<OpportunityNote> Notes { get; set; } = [];
}
