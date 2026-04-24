namespace EFStudio.Sample.Sqlite.Models;

public class CustomerSubscription
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int? ProjectId { get; set; }
    public DateOnly StartedOn { get; set; }
    public DateOnly? RenewsOn { get; set; }
    public DateOnly? CancelledOn { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public bool AutoRenew { get; set; }
    public Customer? Customer { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public Project? Project { get; set; }
}
