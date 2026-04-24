namespace EFStudio.Sample.Sqlite.Models;

public class Invoice
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? ProjectId { get; set; }
    public required string InvoiceNumber { get; set; }
    public DateOnly IssuedOn { get; set; }
    public DateOnly DueOn { get; set; }
    public DateOnly? PaidOn { get; set; }
    public required string Currency { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public Guid ExternalReference { get; set; }
    public Company? Company { get; set; }
    public Project? Project { get; set; }
    public List<InvoiceLine> Lines { get; set; } = [];
}
