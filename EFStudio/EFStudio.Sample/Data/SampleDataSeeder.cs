using Bogus;
using EFStudio.Sample.Sqlite.Models;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Sample.Sqlite.Data;

public static class SampleDataSeeder
{
    private const int CompanyCount = 8;
    private const int TagCount = 24;
    private const int OfficesPerCompany = 3;
    private const int MinimumEmployeesPerCompany = 28;
    private const int MaximumEmployeesPerCompany = 44;
    private const int MinimumDepartmentsPerCompany = 5;
    private const int MaximumDepartmentsPerCompany = 7;
    private const int MinimumProjectsPerCompany = 10;
    private const int MaximumProjectsPerCompany = 18;
    private const int MinimumMembersPerProject = 4;
    private const int MaximumMembersPerProject = 9;
    private const int MinimumWorkItemsPerProject = 8;
    private const int MaximumWorkItemsPerProject = 18;
    private const int MinimumCommentsPerWorkItem = 1;
    private const int MaximumCommentsPerWorkItem = 5;
    private const int MinimumAssetsPerCompany = 18;
    private const int MaximumAssetsPerCompany = 36;
    private const int MinimumInvoicesPerCompany = 8;
    private const int MaximumInvoicesPerCompany = 16;
    private const int MinimumPostsPerCompany = 10;
    private const int MaximumPostsPerCompany = 22;
    private const int MinimumAuditLogsPerCompany = 180;
    private const int MaximumAuditLogsPerCompany = 320;
    private const int SeedValue = 104729;
    private const int SmallBinarySize = 32;
    private const decimal TaxRate = 0.0825m;

    private static readonly string[] Languages = ["en", "es", "pt", "de", "fr"];
    private static readonly string[] Themes = ["system", "light", "dark", "high-contrast"];
    private static readonly string[] MembershipRoles = ["Owner", "Lead", "Engineer", "Analyst", "Designer", "QA"];
    private static readonly string[] DepartmentNames =
    [
        "Engineering", "Product", "Design", "Operations", "Finance", "People", "Sales", "Support"
    ];
    private static readonly string[] TagNames =
    [
        "frontend", "backend", "api", "data", "infra", "security", "billing", "mobile",
        "design", "analytics", "urgent", "blocked", "compliance", "performance", "ux", "docs",
        "migration", "testing", "release", "partner", "support", "ops", "quality", "research"
    ];
    private static readonly (string CountryCode, string TimeZone, double Latitude, double Longitude, string City)[] OfficeLocations =
    [
        ("US", "America/New_York", 40.7128, -74.0060, "New York"),
        ("US", "America/Chicago", 41.8781, -87.6298, "Chicago"),
        ("US", "America/Los_Angeles", 34.0522, -118.2437, "Los Angeles"),
        ("CA", "America/Toronto", 43.6532, -79.3832, "Toronto"),
        ("DE", "Europe/Berlin", 52.5200, 13.4050, "Berlin"),
        ("GB", "Europe/London", 51.5072, -0.1276, "London"),
        ("BR", "America/Sao_Paulo", -23.5505, -46.6333, "Sao Paulo"),
        ("AU", "Australia/Sydney", -33.8688, 151.2093, "Sydney")
    ];

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Companies.AnyAsync())
        {
            return;
        }

        Randomizer.Seed = new Random(SeedValue);
        var faker = new Faker("en");
        var utcNow = DateTime.UtcNow;

        var companies = BuildCompanies(faker, utcNow);
        var offices = BuildOffices(faker, companies);
        var departments = BuildDepartments(faker, companies, utcNow);
        var employees = BuildEmployees(faker, companies, departments, offices, utcNow);
        var employeeProfiles = BuildEmployeeProfiles(faker, employees);
        var tags = BuildTags(faker, utcNow);
        var projects = BuildProjects(faker, companies, departments, utcNow);
        var memberships = BuildProjectMemberships(faker, projects, employees, utcNow);
        var workItems = BuildWorkItems(faker, projects, memberships, utcNow);
        var comments = BuildComments(faker, workItems, memberships, utcNow);
        var workItemTags = BuildWorkItemTags(faker, workItems, tags);
        var posts = BuildPosts(faker, companies, employees, utcNow);
        var invoices = BuildInvoices(faker, companies, projects, utcNow);
        var invoiceLines = BuildInvoiceLines(faker, invoices);
        var assets = BuildAssets(faker, companies, employees, utcNow);
        var auditLogs = BuildAuditLogs(faker, companies, employees, utcNow);

        context.AddRange(companies);
        context.AddRange(offices);
        context.AddRange(departments);
        context.AddRange(employees);
        context.AddRange(employeeProfiles);
        context.AddRange(tags);
        context.AddRange(projects);
        context.AddRange(memberships);
        context.AddRange(workItems);
        context.AddRange(comments);
        context.AddRange(workItemTags);
        context.AddRange(posts);
        context.AddRange(invoices);
        context.AddRange(invoiceLines);
        context.AddRange(assets);
        context.AddRange(auditLogs);

        await context.SaveChangesAsync();
    }

    private static List<Company> BuildCompanies(Faker faker, DateTime utcNow)
    {
        return Enumerable.Range(1, CompanyCount)
            .Select(index =>
            {
                var companyName = faker.Company.CompanyName();
                return new Company
                {
                    Id = index,
                    Name = companyName,
                    Slug = ToSlug(companyName),
                    Industry = faker.Company.CatchPhrase(),
                    FoundedOn = DateOnly.FromDateTime(faker.Date.Past(15, utcNow.AddYears(-5))),
                    AnnualRevenue = faker.Random.Decimal(2_500_000m, 15_000_000m),
                    IsActive = faker.Random.Bool(0.88f),
                    SupportEmail = faker.Internet.Email(provider: "sample.efstudio.dev")
                };
            })
            .ToList();
    }

    private static List<Office> BuildOffices(Faker faker, List<Company> companies)
    {
        return companies.SelectMany((company, companyIndex) =>
                Enumerable.Range(0, OfficesPerCompany).Select(officeIndex =>
                {
                    var location = OfficeLocations[(companyIndex + officeIndex) % OfficeLocations.Length];
                    return new Office
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        Name = officeIndex == 0 ? $"{location.City} HQ" : $"{location.City} {faker.Commerce.Department()} hub",
                        CountryCode = location.CountryCode,
                        TimeZone = location.TimeZone,
                        Latitude = location.Latitude + faker.Random.Double(0.01, 0.09),
                        Longitude = location.Longitude - faker.Random.Double(0.01, 0.09),
                        FloorCount = (short)faker.Random.Int(2, 18),
                        IsHeadquarters = officeIndex == 0
                    };
                }))
            .ToList();
    }

    private static List<Department> BuildDepartments(Faker faker, List<Company> companies, DateTime utcNow)
    {
        var nextDepartmentId = 1;

        return companies.SelectMany(company =>
                DepartmentNames
                    .Take(faker.Random.Int(MinimumDepartmentsPerCompany, MaximumDepartmentsPerCompany))
                    .Select((name, index) => new Department
                    {
                        Id = nextDepartmentId++,
                        CompanyId = company.Id,
                        Name = name,
                        CostCenter = $"{company.Slug[..Math.Min(4, company.Slug.Length)].ToUpperInvariant()}-{index + 10:D3}",
                        Budget = faker.Random.Decimal(250_000m, 1_250_000m),
                        CreatedAtUtc = faker.Date.Past(6, utcNow.AddYears(-1)).ToUniversalTime()
                    }))
            .ToList();
    }

    private static List<Employee> BuildEmployees(
        Faker faker,
        List<Company> companies,
        List<Department> departments,
        List<Office> offices,
        DateTime utcNow)
    {
        var employees = new List<Employee>();
        long nextEmployeeNumber = 100_000;

        foreach (var company in companies)
        {
            var companyDepartments = departments.Where(x => x.CompanyId == company.Id).ToList();
            var companyOffices = offices.Where(x => x.CompanyId == company.Id).ToList();
            var employeeCount = faker.Random.Int(MinimumEmployeesPerCompany, MaximumEmployeesPerCompany);

            for (var employeeIndex = 0; employeeIndex < employeeCount; employeeIndex++)
            {
                var firstName = faker.Name.FirstName();
                var lastName = faker.Name.LastName();

                employees.Add(new Employee
                {
                    Id = Guid.NewGuid(),
                    CompanyId = company.Id,
                    DepartmentId = faker.PickRandom(companyDepartments).Id,
                    OfficeId = faker.Random.Bool(0.85f) ? faker.PickRandom(companyOffices).Id : null,
                    ManagerId = null,
                    EmployeeNumber = nextEmployeeNumber++,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = faker.Internet.Email(firstName, lastName, $"{company.Slug}.dev").ToLowerInvariant(),
                    BirthDate = DateOnly.FromDateTime(faker.Date.Past(35, utcNow.AddYears(-23))),
                    HireDateUtc = faker.Date.Past(8, utcNow.AddMonths(-2)).ToUniversalTime(),
                    LastSeenAt = faker.Date.RecentOffset(30),
                    PreferredStartTime = TimeOnly.FromDateTime(faker.Date.Between(utcNow.Date.AddHours(6), utcNow.Date.AddHours(10).AddMinutes(30))),
                    Salary = faker.Random.Decimal(52_000m, 162_000m),
                    BonusRate = faker.Random.Float(0.00f, 0.25f),
                    VacationDays = faker.Random.Byte(10, 30),
                    IsRemote = faker.Random.Bool(0.35f),
                    AccessLevel = faker.PickRandom<AccessLevel>(),
                    ProfileImage = faker.Random.Bytes(SmallBinarySize),
                    MetadataJson = $$"""{"desk":"{{faker.Random.Int(100, 999)}}","workspace":"{{faker.PickRandom("focus","collab","hybrid")}}","badgeEnabled":{{faker.Random.Bool(0.9f).ToString().ToLowerInvariant()}}}"""
                });
            }

            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).OrderBy(x => x.HireDateUtc).ToList();
            for (var index = 0; index < companyEmployees.Count; index++)
            {
                if (index < 3 || faker.Random.Bool(0.25f))
                {
                    continue;
                }

                companyEmployees[index].ManagerId = faker.PickRandom(companyEmployees.Take(index).ToList()).Id;
            }
        }

        return employees;
    }

    private static List<EmployeeProfile> BuildEmployeeProfiles(Faker faker, List<Employee> employees)
    {
        var nextProfileId = 1;

        return employees.Select(employee => new EmployeeProfile
        {
            Id = nextProfileId++,
            EmployeeId = employee.Id,
            Biography = faker.Lorem.Paragraph(),
            EmergencyContactName = faker.Name.FullName(),
            EmergencyContactPhone = faker.Phone.PhoneNumber("+1-###-###-####"),
            PreferredLanguage = faker.PickRandom(Languages),
            NotifyByEmail = faker.Random.Bool(0.9f),
            NotifyBySms = faker.Random.Bool(0.45f),
            Theme = faker.PickRandom(Themes)
        }).ToList();
    }

    private static List<Tag> BuildTags(Faker faker, DateTime utcNow)
    {
        return TagNames.Take(TagCount).Select((tagName, index) => new Tag
        {
            Id = index + 1,
            Name = tagName,
            HexColor = $"#{faker.Random.Int(0, 0xFFFFFF):X6}",
            CreatedAtUtc = utcNow.AddDays(-(index + 1) * 6)
        }).ToList();
    }

    private static List<Project> BuildProjects(
        Faker faker,
        List<Company> companies,
        List<Department> departments,
        DateTime utcNow)
    {
        var nextProjectId = 1;

        return companies.SelectMany(company =>
            {
                var companyDepartments = departments.Where(x => x.CompanyId == company.Id).ToList();
                var projectCount = faker.Random.Int(MinimumProjectsPerCompany, MaximumProjectsPerCompany);

                return Enumerable.Range(0, projectCount).Select(projectIndex =>
                {
                    var startDate = DateOnly.FromDateTime(faker.Date.Past(2, utcNow.AddMonths(-1)));

                    return new Project
                    {
                        Id = nextProjectId++,
                        CompanyId = company.Id,
                        DepartmentId = faker.Random.Bool(0.85f) ? faker.PickRandom(companyDepartments).Id : null,
                        Code = $"{company.Slug[..Math.Min(3, company.Slug.Length)].ToUpperInvariant()}-{projectIndex + 1:D3}",
                        Name = faker.Company.CatchPhrase(),
                        Description = faker.Lorem.Paragraphs(2),
                        StartDate = startDate,
                        EndDate = faker.Random.Bool(0.3f) ? startDate.AddDays(faker.Random.Int(60, 240)) : null,
                        EstimatedHours = Math.Round(faker.Random.Double(120, 1320), 2),
                        SpentBudget = faker.Random.Decimal(15_000m, 465_000m),
                        Status = faker.PickRandom<ProjectStatus>(),
                        Priority = (short)faker.Random.Int(1, 5),
                        IsBillable = faker.Random.Bool(0.72f)
                    };
                });
            })
            .ToList();
    }

    private static List<ProjectMembership> BuildProjectMemberships(
        Faker faker,
        List<Project> projects,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextMembershipId = 1;
        var memberships = new List<ProjectMembership>();

        foreach (var project in projects)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == project.CompanyId).ToList();
            var memberCount = Math.Min(companyEmployees.Count, faker.Random.Int(MinimumMembersPerProject, MaximumMembersPerProject));
            var selectedEmployees = companyEmployees.OrderBy(_ => faker.Random.Int()).Take(memberCount).ToList();

            memberships.AddRange(selectedEmployees.Select((employee, index) => new ProjectMembership
            {
                Id = nextMembershipId++,
                ProjectId = project.Id,
                EmployeeId = employee.Id,
                Role = faker.PickRandom(MembershipRoles),
                AllocationPercent = faker.Random.Float(0.2f, 0.9f),
                JoinedAtUtc = faker.Date.Past(2, utcNow.AddMonths(-1)).ToUniversalTime(),
                IsPrimaryContact = index == 0
            }));
        }

        return memberships;
    }

    private static List<WorkItem> BuildWorkItems(
        Faker faker,
        List<Project> projects,
        List<ProjectMembership> memberships,
        DateTime utcNow)
    {
        long nextWorkItemId = 10_000;
        var workItems = new List<WorkItem>();

        foreach (var project in projects)
        {
            var memberEmployeeIds = memberships
                .Where(x => x.ProjectId == project.Id)
                .Select(x => x.EmployeeId)
                .ToList();

            var projectWorkItems = new List<WorkItem>();
            var workItemCount = faker.Random.Int(MinimumWorkItemsPerProject, MaximumWorkItemsPerProject);

            for (var index = 0; index < workItemCount; index++)
            {
                var createdAt = faker.Date.Past(1, utcNow.AddDays(-5)).ToUniversalTime();

                var workItem = new WorkItem
                {
                    Id = nextWorkItemId++,
                    ProjectId = project.Id,
                    AssigneeId = faker.Random.Bool(0.85f) ? faker.PickRandom(memberEmployeeIds) : null,
                    ReporterId = faker.PickRandom(memberEmployeeIds),
                    ParentWorkItemId = index > 2 && faker.Random.Bool(0.30f)
                        ? faker.PickRandom(projectWorkItems).Id
                        : null,
                    Title = faker.Hacker.Phrase(),
                    Description = faker.Lorem.Paragraphs(2),
                    Type = faker.PickRandom<WorkItemType>(),
                    State = faker.PickRandom<WorkItemState>(),
                    StoryPoints = faker.Random.Bool(0.75f) ? (byte?)faker.Random.Byte(1, 13) : null,
                    OriginalEstimateHours = faker.Random.Decimal(2m, 82m),
                    RemainingHours = faker.Random.Decimal(0m, 40m),
                    DueAt = faker.Random.Bool(0.70f)
                        ? new DateTimeOffset(createdAt.AddDays(faker.Random.Int(5, 120)), TimeSpan.Zero)
                        : null,
                    CompletedAtUtc = faker.Random.Bool(0.20f) ? faker.Date.Between(createdAt, utcNow).ToUniversalTime() : null
                };

                projectWorkItems.Add(workItem);
                workItems.Add(workItem);
            }
        }

        return workItems;
    }

    private static List<WorkComment> BuildComments(
        Faker faker,
        List<WorkItem> workItems,
        List<ProjectMembership> memberships,
        DateTime utcNow)
    {
        long nextCommentId = 50_000;
        var comments = new List<WorkComment>();

        foreach (var workItem in workItems)
        {
            var projectMembers = memberships
                .Where(x => x.ProjectId == workItem.ProjectId)
                .Select(x => x.EmployeeId)
                .ToList();

            var commentCount = faker.Random.Int(MinimumCommentsPerWorkItem, MaximumCommentsPerWorkItem);

            comments.AddRange(Enumerable.Range(0, commentCount).Select(_ => new WorkComment
            {
                Id = nextCommentId++,
                WorkItemId = workItem.Id,
                AuthorId = faker.PickRandom(projectMembers),
                Body = faker.Lorem.Paragraph(),
                CreatedAt = new DateTimeOffset(faker.Date.Recent(120, utcNow), TimeSpan.Zero),
                IsInternal = faker.Random.Bool(0.35f)
            }));
        }

        return comments;
    }

    private static List<WorkItemTag> BuildWorkItemTags(Faker faker, List<WorkItem> workItems, List<Tag> tags)
    {
        var nextWorkItemTagId = 1;
        var workItemTags = new List<WorkItemTag>();

        foreach (var workItem in workItems)
        {
            var selectedTags = tags.OrderBy(_ => faker.Random.Int()).Take(faker.Random.Int(1, 4)).ToList();

            workItemTags.AddRange(selectedTags.Select(tag => new WorkItemTag
            {
                Id = nextWorkItemTagId++,
                WorkItemId = workItem.Id,
                TagId = tag.Id
            }));
        }

        return workItemTags;
    }

    private static List<Post> BuildPosts(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        var nextPostId = 1;
        var posts = new List<Post>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var postCount = faker.Random.Int(MinimumPostsPerCompany, MaximumPostsPerCompany);

            posts.AddRange(Enumerable.Range(0, postCount).Select(_ => new Post
            {
                Id = nextPostId++,
                AuthorId = faker.PickRandom(companyEmployees).Id,
                CompanyId = company.Id,
                Title = faker.Lorem.Sentence(6),
                Summary = faker.Lorem.Sentence(18),
                Content = faker.Lorem.Paragraphs(4),
                PublishedAtUtc = faker.Random.Bool(0.82f) ? faker.Date.Past(2, utcNow).ToUniversalTime() : null,
                ViewCount = faker.Random.Int(20, 50_000),
                AverageReadTimeMinutes = (short)faker.Random.Int(2, 18),
                IsFeatured = faker.Random.Bool(0.12f)
            }));
        }

        return posts;
    }

    private static List<Invoice> BuildInvoices(Faker faker, List<Company> companies, List<Project> projects, DateTime utcNow)
    {
        var nextInvoiceId = 1;
        var invoices = new List<Invoice>();

        foreach (var company in companies)
        {
            var companyProjects = projects.Where(x => x.CompanyId == company.Id).ToList();
            var invoiceCount = faker.Random.Int(MinimumInvoicesPerCompany, MaximumInvoicesPerCompany);

            for (var index = 0; index < invoiceCount; index++)
            {
                var issuedOn = DateOnly.FromDateTime(faker.Date.Past(1, utcNow));

                invoices.Add(new Invoice
                {
                    Id = nextInvoiceId++,
                    CompanyId = company.Id,
                    ProjectId = faker.Random.Bool(0.8f) ? faker.PickRandom(companyProjects).Id : null,
                    InvoiceNumber = $"{company.Slug[..Math.Min(4, company.Slug.Length)].ToUpperInvariant()}-{utcNow.Year}-{index + 1:D4}",
                    IssuedOn = issuedOn,
                    DueOn = issuedOn.AddDays(faker.Random.Int(15, 45)),
                    PaidOn = faker.Random.Bool(0.55f) ? issuedOn.AddDays(faker.Random.Int(10, 50)) : null,
                    Currency = "USD",
                    Subtotal = 0m,
                    TaxRate = TaxRate,
                    Total = 0m,
                    Notes = faker.Random.Bool(0.35f) ? faker.Lorem.Sentence(12) : null,
                    ExternalReference = Guid.NewGuid()
                });
            }
        }

        return invoices;
    }

    private static List<InvoiceLine> BuildInvoiceLines(Faker faker, List<Invoice> invoices)
    {
        var nextInvoiceLineId = 1;
        var invoiceLines = new List<InvoiceLine>();

        foreach (var invoice in invoices)
        {
            var lineCount = faker.Random.Int(2, 6);
            decimal subtotal = 0m;

            for (short sortOrder = 1; sortOrder <= lineCount; sortOrder++)
            {
                var quantity = faker.Random.Decimal(1m, 80m);
                var unitPrice = faker.Random.Decimal(25m, 475m);
                var lineTotal = decimal.Round(quantity * unitPrice, 2);
                subtotal += lineTotal;

                invoiceLines.Add(new InvoiceLine
                {
                    Id = nextInvoiceLineId++,
                    InvoiceId = invoice.Id,
                    Description = faker.Commerce.ProductName(),
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal,
                    SortOrder = sortOrder
                });
            }

            invoice.Subtotal = decimal.Round(subtotal, 2);
            invoice.Total = decimal.Round(subtotal * (1 + invoice.TaxRate), 2);
        }

        return invoiceLines;
    }

    private static List<Asset> BuildAssets(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        var assets = new List<Asset>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var assetCount = faker.Random.Int(MinimumAssetsPerCompany, MaximumAssetsPerCompany);

            assets.AddRange(Enumerable.Range(0, assetCount).Select(index => new Asset
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                AssignedEmployeeId = faker.Random.Bool(0.7f) ? faker.PickRandom(companyEmployees).Id : null,
                SerialNumber = $"{company.Slug[..Math.Min(3, company.Slug.Length)].ToUpperInvariant()}-AST-{index + 1:D5}",
                AssetType = faker.PickRandom<AssetType>(),
                PurchasedAtUtc = faker.Date.Past(5, utcNow.AddMonths(-2)).ToUniversalTime(),
                WarrantyExpiresOn = DateOnly.FromDateTime(faker.Date.Future(3, utcNow)),
                PurchasePrice = faker.Random.Decimal(150m, 3_150m),
                DepreciationRate = Math.Round(faker.Random.Double(0.01, 0.35), 3),
                IsRetired = faker.Random.Bool(0.12f),
                LastAuditAt = new DateTimeOffset(faker.Date.Recent(240, utcNow), TimeSpan.Zero),
                SpecificationsJson = $$"""{"memoryGb":{{faker.Random.Int(8, 64)}},"storageGb":{{faker.Random.Int(128, 2048)}},"wireless":{{faker.Random.Bool(0.8f).ToString().ToLowerInvariant()}}}"""
            }));
        }

        return assets;
    }

    private static List<AuditLog> BuildAuditLogs(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        long nextAuditLogId = 900_000;
        var entityNames = new[] { "Project", "WorkItem", "Invoice", "Employee", "Asset", "Post" };
        var auditLogs = new List<AuditLog>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var auditCount = faker.Random.Int(MinimumAuditLogsPerCompany, MaximumAuditLogsPerCompany);

            auditLogs.AddRange(Enumerable.Range(0, auditCount).Select(_ => new AuditLog
            {
                Id = nextAuditLogId++,
                CompanyId = company.Id,
                EmployeeId = faker.Random.Bool(0.94f) ? faker.PickRandom(companyEmployees).Id : null,
                EntityName = faker.PickRandom(entityNames),
                EntityId = faker.Random.Int(1, 2_500).ToString(),
                Action = faker.PickRandom<AuditAction>(),
                OccurredAt = new DateTimeOffset(faker.Date.Recent(365, utcNow), TimeSpan.Zero),
                IpAddress = faker.Internet.Ip(),
                CorrelationId = Guid.NewGuid(),
                ChangesJson = $$"""{"before":"{{faker.Lorem.Word()}}","after":"{{faker.Lorem.Word()}}","source":"bogus"}""",
                Success = faker.Random.Bool(0.93f)
            }));
        }

        return auditLogs;
    }

    private static string ToSlug(string value)
    {
        var normalized = value.ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();

        return string.Join(
            "-",
            new string(normalized)
                .Split('-', StringSplitOptions.RemoveEmptyEntries)
        );
    }
}
