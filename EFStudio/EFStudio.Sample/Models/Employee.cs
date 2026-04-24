namespace EFStudio.Sample.Sqlite.Models;

public class Employee
{
    public Guid Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public Guid? OfficeId { get; set; }
    public Guid? ManagerId { get; set; }
    public long EmployeeNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime HireDateUtc { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public TimeOnly? PreferredStartTime { get; set; }
    public decimal Salary { get; set; }
    public float BonusRate { get; set; }
    public byte VacationDays { get; set; }
    public bool IsRemote { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public byte[]? ProfileImage { get; set; }
    public string? MetadataJson { get; set; }
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public Office? Office { get; set; }
    public Employee? Manager { get; set; }
    public EmployeeProfile? Profile { get; set; }
    public List<Employee> DirectReports { get; set; } = [];
    public List<ProjectMembership> ProjectMemberships { get; set; } = [];
    public List<WorkItem> AssignedWorkItems { get; set; } = [];
    public List<WorkItem> ReportedWorkItems { get; set; } = [];
    public List<WorkComment> Comments { get; set; } = [];
    public List<Post> Posts { get; set; } = [];
    public List<Asset> Assets { get; set; } = [];
    public List<Customer> ManagedCustomers { get; set; } = [];
    public List<SalesOpportunity> OwnedOpportunities { get; set; } = [];
    public List<OpportunityNote> OpportunityNotes { get; set; } = [];
    public List<SupportTicket> AssignedSupportTickets { get; set; } = [];
    public List<SupportTicketComment> SupportTicketComments { get; set; } = [];
    public List<PurchaseOrder> RequestedPurchaseOrders { get; set; } = [];
    public List<PurchaseOrder> ApprovedPurchaseOrders { get; set; } = [];
    public List<ExpenseClaim> ExpenseClaims { get; set; } = [];
    public List<ExpenseClaim> ApprovedExpenseClaims { get; set; } = [];
    public List<TeamMembership> TeamMemberships { get; set; } = [];
    public List<CourseEnrollment> CourseEnrollments { get; set; } = [];
    public List<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; } = [];
    public List<CompanyEvent> HostedEvents { get; set; } = [];
    public List<EventAttendance> EventAttendances { get; set; } = [];
    public List<ApiCredential> ApiCredentials { get; set; } = [];
    public List<ReleaseNote> ReleaseNotes { get; set; } = [];
    public List<TimeEntry> TimeEntries { get; set; } = [];
    public List<EmployeeBenefitEnrollment> BenefitEnrollments { get; set; } = [];
    public List<LeaveRequest> LeaveRequests { get; set; } = [];
    public List<LeaveRequest> ApprovedLeaveRequests { get; set; } = [];
    public List<Candidate> RecruitedCandidates { get; set; } = [];
    public List<InterviewSession> Interviews { get; set; } = [];
    public List<AuditLog> AuditLogs { get; set; } = [];
}
