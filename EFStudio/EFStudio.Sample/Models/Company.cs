namespace EFStudio.Sample.Sqlite.Models;

public class Company
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Industry { get; set; }
    public DateOnly FoundedOn { get; set; }
    public decimal AnnualRevenue { get; set; }
    public bool IsActive { get; set; }
    public string? SupportEmail { get; set; }
    public List<Office> Offices { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
    public List<Post> Posts { get; set; } = [];
    public List<Invoice> Invoices { get; set; } = [];
    public List<Asset> Assets { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<SubscriptionPlan> SubscriptionPlans { get; set; } = [];
    public List<Vendor> Vendors { get; set; } = [];
    public List<Team> Teams { get; set; } = [];
    public List<TrainingCourse> TrainingCourses { get; set; } = [];
    public List<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; } = [];
    public List<CompanyEvent> Events { get; set; } = [];
    public List<IntegrationEndpoint> IntegrationEndpoints { get; set; } = [];
    public List<BenefitPlan> BenefitPlans { get; set; } = [];
    public List<AuditLog> AuditLogs { get; set; } = [];
}
