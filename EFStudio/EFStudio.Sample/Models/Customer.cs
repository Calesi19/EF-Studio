namespace EFStudio.Sample.Sqlite.Models;

public class Customer
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Guid? AccountManagerId { get; set; }
    public required string Name { get; set; }
    public required string EmailDomain { get; set; }
    public string? CustomerTier { get; set; }
    public string? CountryCode { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public Company? Company { get; set; }
    public Employee? AccountManager { get; set; }
    public List<CustomerContact> Contacts { get; set; } = [];
    public List<CustomerSubscription> Subscriptions { get; set; } = [];
    public List<SalesOpportunity> Opportunities { get; set; } = [];
    public List<SupportTicket> SupportTickets { get; set; } = [];
}
