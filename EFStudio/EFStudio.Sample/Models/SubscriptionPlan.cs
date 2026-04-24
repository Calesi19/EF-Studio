namespace EFStudio.Sample.Sqlite.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public required string Name { get; set; }
    public required string BillingInterval { get; set; }
    public decimal MonthlyPrice { get; set; }
    public short SeatsIncluded { get; set; }
    public short TrialDays { get; set; }
    public bool IsLegacy { get; set; }
    public Company? Company { get; set; }
    public List<CustomerSubscription> CustomerSubscriptions { get; set; } = [];
}
