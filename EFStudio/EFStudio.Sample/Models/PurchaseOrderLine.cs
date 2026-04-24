namespace EFStudio.Sample.Sqlite.Models;

public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public required string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    public short SortOrder { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
}
