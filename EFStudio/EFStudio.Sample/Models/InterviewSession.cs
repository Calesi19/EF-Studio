namespace EFStudio.Sample.Sqlite.Models;

public class InterviewSession
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public Guid InterviewerEmployeeId { get; set; }
    public DateTimeOffset ScheduledAtUtc { get; set; }
    public short DurationMinutes { get; set; }
    public InterviewFormat Format { get; set; }
    public decimal? Score { get; set; }
    public string? Notes { get; set; }
    public Candidate? Candidate { get; set; }
    public Employee? InterviewerEmployee { get; set; }
}
