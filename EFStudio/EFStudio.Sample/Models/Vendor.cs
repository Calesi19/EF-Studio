namespace EFStudio.Sample.Sqlite.Models;

public class Vendor
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public string? SupportEmail { get; set; }
    public string? CountryCode { get; set; }
    public decimal? Rating { get; set; }
    public bool IsPreferred { get; set; }
    public Company? Company { get; set; }
    public List<PurchaseOrder> PurchaseOrders { get; set; } = [];
}
