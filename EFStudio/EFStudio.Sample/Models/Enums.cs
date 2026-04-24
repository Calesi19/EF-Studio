namespace EFStudio.Sample.Sqlite.Models;

public enum AccessLevel
{
    Guest,
    Member,
    Manager,
    Director,
    Executive
}

public enum ProjectStatus
{
    Proposed,
    Active,
    Paused,
    Completed,
    Cancelled
}

public enum WorkItemType
{
    Epic,
    Story,
    Task,
    Bug,
    Improvement
}

public enum WorkItemState
{
    Backlog,
    Ready,
    InProgress,
    Blocked,
    InReview,
    Done
}

public enum AssetType
{
    Laptop,
    Monitor,
    Phone,
    Tablet,
    Desk,
    Chair,
    Badge,
    Vehicle
}

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Login,
    Export,
    Assign,
    Approve
}

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Paused,
    Cancelled
}

public enum OpportunityStage
{
    Prospect,
    Qualified,
    Proposal,
    Negotiation,
    Won,
    Lost
}

public enum TicketStatus
{
    New,
    Open,
    WaitingOnCustomer,
    Escalated,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum PurchaseOrderStatus
{
    Draft,
    Submitted,
    Approved,
    Received,
    Cancelled
}

public enum ExpenseStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Reimbursed
}

public enum EnrollmentStatus
{
    Enrolled,
    InProgress,
    Completed,
    Cancelled
}

public enum AttendanceStatus
{
    Registered,
    Attended,
    Cancelled,
    NoShow
}

public enum CredentialProvider
{
    GitHub,
    Slack,
    Stripe,
    Jira,
    Salesforce,
    HubSpot
}

public enum ReleaseKind
{
    Major,
    Minor,
    Patch,
    Hotfix
}

public enum FeatureFlagType
{
    Release,
    Experiment,
    Operational,
    Permission
}

public enum SprintGoalStatus
{
    Planned,
    AtRisk,
    Achieved,
    Missed
}

public enum TimeEntrySource
{
    Manual,
    Timer,
    Imported,
    Mobile
}

public enum LeaveRequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum CandidateStage
{
    Applied,
    Screening,
    Interviewing,
    Offer,
    Hired,
    Rejected
}

public enum InterviewFormat
{
    Phone,
    Video,
    Onsite,
    Panel
}
