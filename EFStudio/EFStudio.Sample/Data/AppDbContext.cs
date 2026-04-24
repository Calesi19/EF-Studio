using EFStudio.Sample.Sqlite.Models;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Sample.Sqlite.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMembership> ProjectMemberships => Set<ProjectMembership>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<WorkComment> WorkComments => Set<WorkComment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<WorkItemTag> WorkItemTags => Set<WorkItemTag>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<CustomerSubscription> CustomerSubscriptions => Set<CustomerSubscription>();
    public DbSet<SalesOpportunity> SalesOpportunities => Set<SalesOpportunity>();
    public DbSet<OpportunityNote> OpportunityNotes => Set<OpportunityNote>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportTicketComment> SupportTicketComments => Set<SupportTicketComment>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<ExpenseClaim> ExpenseClaims => Set<ExpenseClaim>();
    public DbSet<ExpenseLine> ExpenseLines => Set<ExpenseLine>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMembership> TeamMemberships => Set<TeamMembership>();
    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles => Set<KnowledgeBaseArticle>();
    public DbSet<CompanyEvent> CompanyEvents => Set<CompanyEvent>();
    public DbSet<EventAttendance> EventAttendances => Set<EventAttendance>();
    public DbSet<IntegrationEndpoint> IntegrationEndpoints => Set<IntegrationEndpoint>();
    public DbSet<ApiCredential> ApiCredentials => Set<ApiCredential>();
    public DbSet<ReleaseNote> ReleaseNotes => Set<ReleaseNote>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<SprintAssignment> SprintAssignments => Set<SprintAssignment>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<BenefitPlan> BenefitPlans => Set<BenefitPlan>();
    public DbSet<EmployeeBenefitEnrollment> EmployeeBenefitEnrollments => Set<EmployeeBenefitEnrollment>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.Slug).HasMaxLength(80);
            entity.Property(x => x.Industry).HasMaxLength(80);
            entity.Property(x => x.SupportEmail).HasMaxLength(160);
            entity.Property(x => x.AnnualRevenue).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.CountryCode).HasMaxLength(2);
            entity.Property(x => x.TimeZone).HasMaxLength(60);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.CostCenter).HasMaxLength(20);
            entity.Property(x => x.Budget).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(60);
            entity.Property(x => x.LastName).HasMaxLength(80);
            entity.Property(x => x.Email).HasMaxLength(160);
            entity.Property(x => x.Salary).HasPrecision(18, 2);
            entity.Property(x => x.MetadataJson).HasColumnType("TEXT");
            entity.Property(x => x.AccessLevel).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(x => x.Manager)
                .WithMany(x => x.DirectReports)
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeProfile>(entity =>
        {
            entity.Property(x => x.EmergencyContactName).HasMaxLength(120);
            entity.Property(x => x.EmergencyContactPhone).HasMaxLength(30);
            entity.Property(x => x.PreferredLanguage).HasMaxLength(20);
            entity.Property(x => x.Theme).HasMaxLength(30);

            entity.HasOne(x => x.Employee)
                .WithOne(x => x.Profile)
                .HasForeignKey<EmployeeProfile>(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.EmployeeId).IsUnique();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(20);
            entity.Property(x => x.Name).HasMaxLength(140);
            entity.Property(x => x.Description).HasColumnType("TEXT");
            entity.Property(x => x.SpentBudget).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<ProjectMembership>(entity =>
        {
            entity.Property(x => x.Role).HasMaxLength(40);
            entity.HasIndex(x => new { x.ProjectId, x.EmployeeId }).IsUnique();
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(160);
            entity.Property(x => x.Description).HasColumnType("TEXT");
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.State).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.OriginalEstimateHours).HasPrecision(10, 2);
            entity.Property(x => x.RemainingHours).HasPrecision(10, 2);

            entity.HasOne(x => x.ParentWorkItem)
                .WithMany(x => x.ChildWorkItems)
                .HasForeignKey(x => x.ParentWorkItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Assignee)
                .WithMany(x => x.AssignedWorkItems)
                .HasForeignKey(x => x.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Reporter)
                .WithMany(x => x.ReportedWorkItems)
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkComment>(entity =>
        {
            entity.Property(x => x.Body).HasColumnType("TEXT");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(40);
            entity.Property(x => x.HexColor).HasMaxLength(7);
        });

        modelBuilder.Entity<WorkItemTag>(entity =>
        {
            entity.HasIndex(x => new { x.WorkItemId, x.TagId }).IsUnique();
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(180);
            entity.Property(x => x.Summary).HasMaxLength(280);
            entity.Property(x => x.Content).HasColumnType("TEXT");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(x => x.InvoiceNumber).HasMaxLength(30);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(5, 4);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasColumnType("TEXT");
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.Property(x => x.Quantity).HasPrecision(10, 2);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.Property(x => x.SerialNumber).HasMaxLength(60);
            entity.Property(x => x.AssetType).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.SpecificationsJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.EmailDomain).HasMaxLength(120);
            entity.Property(x => x.CustomerTier).HasMaxLength(20);
            entity.Property(x => x.CountryCode).HasMaxLength(2);

            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();

            entity.HasOne(x => x.AccountManager)
                .WithMany(x => x.ManagedCustomers)
                .HasForeignKey(x => x.AccountManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CustomerContact>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(60);
            entity.Property(x => x.LastName).HasMaxLength(80);
            entity.Property(x => x.Email).HasMaxLength(160);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Title).HasMaxLength(80);
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(80);
            entity.Property(x => x.BillingInterval).HasMaxLength(20);
            entity.Property(x => x.MonthlyPrice).HasPrecision(18, 2);

            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<CustomerSubscription>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.MonthlyRecurringRevenue).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SalesOpportunity>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(180);
            entity.Property(x => x.Stage).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.EstimatedValue).HasPrecision(18, 2);
            entity.Property(x => x.Source).HasMaxLength(40);

            entity.HasOne(x => x.OwnerEmployee)
                .WithMany(x => x.OwnedOpportunities)
                .HasForeignKey(x => x.OwnerEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OpportunityNote>(entity =>
        {
            entity.Property(x => x.Body).HasColumnType("TEXT");

            entity.HasOne(x => x.Author)
                .WithMany(x => x.OpportunityNotes)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.Property(x => x.Subject).HasMaxLength(180);
            entity.Property(x => x.Description).HasColumnType("TEXT");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(x => x.AssignedEmployee)
                .WithMany(x => x.AssignedSupportTickets)
                .HasForeignKey(x => x.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SupportTicketComment>(entity =>
        {
            entity.Property(x => x.Body).HasColumnType("TEXT");

            entity.HasOne(x => x.SupportTicket)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.AuthorEmployee)
                .WithMany(x => x.SupportTicketComments)
                .HasForeignKey(x => x.AuthorEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.CustomerContact)
                .WithMany(x => x.SupportTicketComments)
                .HasForeignKey(x => x.CustomerContactId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.Category).HasMaxLength(60);
            entity.Property(x => x.SupportEmail).HasMaxLength(160);
            entity.Property(x => x.CountryCode).HasMaxLength(2);
            entity.Property(x => x.Rating).HasPrecision(4, 2);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.Property(x => x.OrderNumber).HasMaxLength(40);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.ExternalReference).HasMaxLength(80);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(x => x.OrderNumber).IsUnique();

            entity.HasOne(x => x.RequestedByEmployee)
                .WithMany(x => x.RequestedPurchaseOrders)
                .HasForeignKey(x => x.RequestedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByEmployee)
                .WithMany(x => x.ApprovedPurchaseOrders)
                .HasForeignKey(x => x.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.Property(x => x.Quantity).HasPrecision(10, 2);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ExpenseClaim>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(160);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasColumnType("TEXT");

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.ExpenseClaims)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Approver)
                .WithMany(x => x.ApprovedExpenseClaims)
                .HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExpenseLine>(entity =>
        {
            entity.Property(x => x.Category).HasMaxLength(60);
            entity.Property(x => x.Merchant).HasMaxLength(120);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.ReceiptUrl).HasMaxLength(240);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.Slug).HasMaxLength(80);
            entity.Property(x => x.FocusArea).HasMaxLength(120);

            entity.HasIndex(x => new { x.CompanyId, x.Slug }).IsUnique();
        });

        modelBuilder.Entity<TeamMembership>(entity =>
        {
            entity.Property(x => x.Role).HasMaxLength(40);
            entity.HasIndex(x => new { x.TeamId, x.EmployeeId }).IsUnique();
        });

        modelBuilder.Entity<TrainingCourse>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(160);
            entity.Property(x => x.DeliveryMethod).HasMaxLength(40);
            entity.Property(x => x.DurationHours).HasPrecision(8, 2);
        });

        modelBuilder.Entity<CourseEnrollment>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Score).HasPrecision(5, 2);
            entity.HasIndex(x => new { x.TrainingCourseId, x.EmployeeId }).IsUnique();
        });

        modelBuilder.Entity<KnowledgeBaseArticle>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(180);
            entity.Property(x => x.Slug).HasMaxLength(120);
            entity.Property(x => x.Summary).HasMaxLength(280);
            entity.Property(x => x.Body).HasColumnType("TEXT");

            entity.HasIndex(x => new { x.CompanyId, x.Slug }).IsUnique();

            entity.HasOne(x => x.Author)
                .WithMany(x => x.KnowledgeBaseArticles)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompanyEvent>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(140);
            entity.Property(x => x.EventType).HasMaxLength(40);

            entity.HasOne(x => x.HostEmployee)
                .WithMany(x => x.HostedEvents)
                .HasForeignKey(x => x.HostEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EventAttendance>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => new { x.CompanyEventId, x.EmployeeId }).IsUnique();

            entity.HasOne(x => x.CompanyEvent)
                .WithMany(x => x.Attendees)
                .HasForeignKey(x => x.CompanyEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.EventAttendances)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IntegrationEndpoint>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.Provider).HasMaxLength(40);
            entity.Property(x => x.BaseUrl).HasMaxLength(200);
        });

        modelBuilder.Entity<ApiCredential>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.KeyPrefix).HasMaxLength(24);

            entity.HasOne(x => x.OwnerEmployee)
                .WithMany(x => x.ApiCredentials)
                .HasForeignKey(x => x.OwnerEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReleaseNote>(entity =>
        {
            entity.Property(x => x.Version).HasMaxLength(20);
            entity.Property(x => x.Title).HasMaxLength(180);
            entity.Property(x => x.Highlights).HasColumnType("TEXT");
            entity.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(x => x.Author)
                .WithMany(x => x.ReleaseNotes)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.FlagType).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(80);
            entity.Property(x => x.Goal).HasMaxLength(240);
            entity.Property(x => x.GoalStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.CapacityHours).HasPrecision(10, 2);
        });

        modelBuilder.Entity<SprintAssignment>(entity =>
        {
            entity.HasIndex(x => new { x.SprintId, x.WorkItemId }).IsUnique();
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.Property(x => x.Hours).HasPrecision(8, 2);
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.Property(x => x.Source).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.TimeEntries)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BenefitPlan>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.Provider).HasMaxLength(120);
            entity.Property(x => x.PlanType).HasMaxLength(40);
            entity.Property(x => x.EmployeeMonthlyCost).HasPrecision(18, 2);
            entity.Property(x => x.EmployerMonthlyContribution).HasPrecision(18, 2);
        });

        modelBuilder.Entity<EmployeeBenefitEnrollment>(entity =>
        {
            entity.Property(x => x.CoverageLevel).HasMaxLength(40);
            entity.HasIndex(x => new { x.BenefitPlanId, x.EmployeeId }).IsUnique();

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.BenefitEnrollments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.Property(x => x.LeaveType).HasMaxLength(40);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Reason).HasMaxLength(240);

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.LeaveRequests)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Approver)
                .WithMany(x => x.ApprovedLeaveRequests)
                .HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(120);
            entity.Property(x => x.Email).HasMaxLength(160);
            entity.Property(x => x.Stage).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Source).HasMaxLength(40);
            entity.Property(x => x.DesiredSalary).HasPrecision(18, 2);

            entity.HasOne(x => x.RecruiterEmployee)
                .WithMany(x => x.RecruitedCandidates)
                .HasForeignKey(x => x.RecruiterEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InterviewSession>(entity =>
        {
            entity.Property(x => x.Format).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Score).HasPrecision(4, 2);
            entity.Property(x => x.Notes).HasMaxLength(240);

            entity.HasOne(x => x.InterviewerEmployee)
                .WithMany(x => x.Interviews)
                .HasForeignKey(x => x.InterviewerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.EntityName).HasMaxLength(80);
            entity.Property(x => x.EntityId).HasMaxLength(80);
            entity.Property(x => x.Action).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.Property(x => x.ChangesJson).HasColumnType("TEXT");
        });
    }
}
