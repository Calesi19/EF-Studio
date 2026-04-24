namespace EFStudio.Sample.Sqlite.Models;

public class PurchaseOrder
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int VendorId { get; set; }
    public Guid RequestedByEmployeeId { get; set; }
    public Guid? ApprovedByEmployeeId { get; set; }
    public Guid? OfficeId { get; set; }
    public required string OrderNumber { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public DateTime OrderedAtUtc { get; set; }
    public DateOnly? ReceivedOn { get; set; }
    public required string Currency { get; set; }
    public decimal Total { get; set; }
    public string? ExternalReference { get; set; }
    public Company? Company { get; set; }
    public Vendor? Vendor { get; set; }
    public Employee? RequestedByEmployee { get; set; }
    public Employee? ApprovedByEmployee { get; set; }
    public Office? Office { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = [];
}
