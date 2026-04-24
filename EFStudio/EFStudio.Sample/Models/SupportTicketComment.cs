namespace EFStudio.Sample.Sqlite.Models;

public class SupportTicketComment
{
    public long Id { get; set; }
    public long SupportTicketId { get; set; }
    public Guid? AuthorEmployeeId { get; set; }
    public int? CustomerContactId { get; set; }
    public required string Body { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public bool IsInternal { get; set; }
    public SupportTicket? SupportTicket { get; set; }
    public Employee? AuthorEmployee { get; set; }
    public CustomerContact? CustomerContact { get; set; }
}
