namespace EFStudio.Sample.Sqlite.Models;

public class Asset
{
    public Guid Id { get; set; }
    public int CompanyId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    public required string SerialNumber { get; set; }
    public AssetType AssetType { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
    public DateOnly? WarrantyExpiresOn { get; set; }
    public decimal PurchasePrice { get; set; }
    public double DepreciationRate { get; set; }
    public bool IsRetired { get; set; }
    public DateTimeOffset? LastAuditAt { get; set; }
    public string? SpecificationsJson { get; set; }
    public Company? Company { get; set; }
    public Employee? AssignedEmployee { get; set; }
}
