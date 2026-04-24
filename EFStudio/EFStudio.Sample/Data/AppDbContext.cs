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
