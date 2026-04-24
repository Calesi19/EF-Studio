namespace EFStudio.Sample.Sqlite.Models;

public class ExpenseLine
{
    public int Id { get; set; }
    public int ExpenseClaimId { get; set; }
    public DateOnly OccurredOn { get; set; }
    public required string Category { get; set; }
    public required string Merchant { get; set; }
    public decimal Amount { get; set; }
    public bool IsBillable { get; set; }
    public string? ReceiptUrl { get; set; }
    public ExpenseClaim? ExpenseClaim { get; set; }
}
