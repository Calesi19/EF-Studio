namespace EFStudio.Sample.Sqlite.Models;

public class InvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public required string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public short SortOrder { get; set; }
    public Invoice? Invoice { get; set; }
}
