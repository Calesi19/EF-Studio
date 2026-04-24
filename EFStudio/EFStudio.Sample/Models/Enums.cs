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
