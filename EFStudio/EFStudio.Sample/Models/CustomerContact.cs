namespace EFStudio.Sample.Sqlite.Models;

public class CustomerContact
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset? LastContactedAt { get; set; }
    public Customer? Customer { get; set; }
    public List<SupportTicketComment> SupportTicketComments { get; set; } = [];
}
