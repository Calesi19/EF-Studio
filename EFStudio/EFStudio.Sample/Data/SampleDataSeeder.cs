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
    private const int MinimumCustomersPerCompany = 10;
    private const int MaximumCustomersPerCompany = 18;
    private const int SubscriptionPlansPerCompany = 4;
    private const int MinimumOpportunitiesPerCompany = 8;
    private const int MaximumOpportunitiesPerCompany = 16;
    private const int MinimumTicketsPerCompany = 10;
    private const int MaximumTicketsPerCompany = 20;
    private const int VendorsPerCompany = 6;
    private const int MinimumPurchaseOrdersPerCompany = 8;
    private const int MaximumPurchaseOrdersPerCompany = 15;
    private const int MinimumExpenseClaimsPerCompany = 10;
    private const int MaximumExpenseClaimsPerCompany = 18;
    private const int TeamsPerCompany = 4;
    private const int CoursesPerCompany = 5;
    private const int ArticlesPerCompany = 8;
    private const int EventsPerCompany = 6;
    private const int EndpointsPerCompany = 4;
    private const int BenefitPlansPerCompany = 3;
    private const int MinimumCandidatesPerCompany = 6;
    private const int MaximumCandidatesPerCompany = 12;
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
    private static readonly string[] CustomerTiers = ["Starter", "Growth", "Scale", "Enterprise"];
    private static readonly string[] OpportunitySources = ["Referral", "Outbound", "Website", "Partner", "Upsell"];
    private static readonly string[] VendorCategories = ["Software", "Hardware", "Facilities", "Marketing", "Security", "Logistics"];
    private static readonly string[] ExpenseCategories = ["Travel", "Meals", "Software", "Training", "Supplies", "Conference"];
    private static readonly string[] TeamFocusAreas = ["Platform", "Growth", "Operations", "Enablement", "Customer Success", "Data"];
    private static readonly string[] CourseDeliveryMethods = ["Self-paced", "Workshop", "Live session", "Certification"];
    private static readonly string[] EventTypes = ["All Hands", "Workshop", "Hack Day", "Town Hall", "Summit", "Meetup"];
    private static readonly string[] IntegrationProviders = ["GitHub", "Slack", "Stripe", "Jira", "Salesforce", "HubSpot"];
    private static readonly string[] BenefitCoverageLevels = ["Employee", "Employee+Spouse", "Employee+Children", "Family"];
    private static readonly string[] LeaveTypes = ["Vacation", "Sick", "Parental", "Volunteer", "Bereavement"];
    private static readonly string[] CandidateSources = ["LinkedIn", "Referral", "Careers Site", "Agency", "Conference"];
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
        var customers = BuildCustomers(faker, companies, employees, utcNow);
        var customerContacts = BuildCustomerContacts(faker, customers, utcNow);
        var subscriptionPlans = BuildSubscriptionPlans(faker, companies);
        var customerSubscriptions = BuildCustomerSubscriptions(faker, customers, subscriptionPlans, projects, utcNow);
        var opportunities = BuildSalesOpportunities(faker, customers, employees, projects, utcNow);
        var opportunityNotes = BuildOpportunityNotes(faker, opportunities, employees, utcNow);
        var supportTickets = BuildSupportTickets(faker, customers, employees, projects, utcNow);
        var supportTicketComments = BuildSupportTicketComments(faker, supportTickets, employees, customerContacts, utcNow);
        var vendors = BuildVendors(faker, companies);
        var purchaseOrders = BuildPurchaseOrders(faker, companies, vendors, employees, offices, utcNow);
        var purchaseOrderLines = BuildPurchaseOrderLines(faker, purchaseOrders);
        var expenseClaims = BuildExpenseClaims(faker, companies, employees, utcNow);
        var expenseLines = BuildExpenseLines(faker, expenseClaims);
        var teams = BuildTeams(faker, companies, departments, utcNow);
        var teamMemberships = BuildTeamMemberships(faker, teams, employees, utcNow);
        var trainingCourses = BuildTrainingCourses(faker, companies, departments, utcNow);
        var courseEnrollments = BuildCourseEnrollments(faker, trainingCourses, employees, utcNow);
        var knowledgeBaseArticles = BuildKnowledgeBaseArticles(faker, companies, employees, utcNow);
        var companyEvents = BuildCompanyEvents(faker, companies, offices, employees, utcNow);
        var eventAttendances = BuildEventAttendances(faker, companyEvents, employees, utcNow);
        var integrationEndpoints = BuildIntegrationEndpoints(faker, companies, utcNow);
        var apiCredentials = BuildApiCredentials(faker, integrationEndpoints, employees, utcNow);
        var releaseNotes = BuildReleaseNotes(faker, projects, memberships, utcNow);
        var featureFlags = BuildFeatureFlags(faker, projects, utcNow);
        var sprints = BuildSprints(faker, projects, utcNow);
        var sprintAssignments = BuildSprintAssignments(faker, sprints, workItems, utcNow);
        var timeEntries = BuildTimeEntries(faker, memberships, workItems, utcNow);
        var benefitPlans = BuildBenefitPlans(faker, companies);
        var benefitEnrollments = BuildBenefitEnrollments(faker, benefitPlans, employees, utcNow);
        var leaveRequests = BuildLeaveRequests(faker, companies, employees, utcNow);
        var candidates = BuildCandidates(faker, companies, departments, employees, utcNow);
        var interviewSessions = BuildInterviewSessions(faker, candidates, employees, utcNow);
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
        context.AddRange(customers);
        context.AddRange(customerContacts);
        context.AddRange(subscriptionPlans);
        context.AddRange(customerSubscriptions);
        context.AddRange(opportunities);
        context.AddRange(opportunityNotes);
        context.AddRange(supportTickets);
        context.AddRange(supportTicketComments);
        context.AddRange(vendors);
        context.AddRange(purchaseOrders);
        context.AddRange(purchaseOrderLines);
        context.AddRange(expenseClaims);
        context.AddRange(expenseLines);
        context.AddRange(teams);
        context.AddRange(teamMemberships);
        context.AddRange(trainingCourses);
        context.AddRange(courseEnrollments);
        context.AddRange(knowledgeBaseArticles);
        context.AddRange(companyEvents);
        context.AddRange(eventAttendances);
        context.AddRange(integrationEndpoints);
        context.AddRange(apiCredentials);
        context.AddRange(releaseNotes);
        context.AddRange(featureFlags);
        context.AddRange(sprints);
        context.AddRange(sprintAssignments);
        context.AddRange(timeEntries);
        context.AddRange(benefitPlans);
        context.AddRange(benefitEnrollments);
        context.AddRange(leaveRequests);
        context.AddRange(candidates);
        context.AddRange(interviewSessions);
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
                    LastSeenAt = faker.Date.RecentOffset(30).ToUniversalTime(),
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

    private static List<Customer> BuildCustomers(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        var nextCustomerId = 1;
        var customers = new List<Customer>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var customerCount = faker.Random.Int(MinimumCustomersPerCompany, MaximumCustomersPerCompany);

            for (var index = 0; index < customerCount; index++)
            {
                var name = faker.Company.CompanyName();
                customers.Add(new Customer
                {
                    Id = nextCustomerId++,
                    CompanyId = company.Id,
                    AccountManagerId = faker.Random.Bool(0.8f) ? faker.PickRandom(companyEmployees).Id : null,
                    Name = name,
                    EmailDomain = $"{ToSlug(name)}.customer.dev",
                    CustomerTier = faker.PickRandom(CustomerTiers),
                    CountryCode = faker.PickRandom(OfficeLocations).CountryCode,
                    CreatedAtUtc = faker.Date.Past(4, utcNow.AddMonths(-2)).ToUniversalTime(),
                    IsActive = faker.Random.Bool(0.85f)
                });
            }
        }

        return customers;
    }

    private static List<CustomerContact> BuildCustomerContacts(Faker faker, List<Customer> customers, DateTime utcNow)
    {
        var nextContactId = 1;
        var contacts = new List<CustomerContact>();

        foreach (var customer in customers)
        {
            var contactCount = faker.Random.Int(2, 4);
            for (var index = 0; index < contactCount; index++)
            {
                var firstName = faker.Name.FirstName();
                var lastName = faker.Name.LastName();
                contacts.Add(new CustomerContact
                {
                    Id = nextContactId++,
                    CustomerId = customer.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = faker.Internet.Email(firstName, lastName, customer.EmailDomain).ToLowerInvariant(),
                    Phone = faker.Phone.PhoneNumber("+1-###-###-####"),
                    Title = faker.Name.JobTitle(),
                    IsPrimary = index == 0,
                    LastContactedAt = faker.Random.Bool(0.75f) ? faker.Date.RecentOffset(90, utcNow).ToUniversalTime() : null
                });
            }
        }

        return contacts;
    }

    private static List<SubscriptionPlan> BuildSubscriptionPlans(Faker faker, List<Company> companies)
    {
        var nextPlanId = 1;
        var planTemplates = new[]
        {
            ("Starter", 49m, (short)5, (short)14),
            ("Growth", 149m, (short)20, (short)21),
            ("Scale", 399m, (short)75, (short)30),
            ("Enterprise", 999m, (short)250, (short)45)
        };

        return companies.SelectMany(company => planTemplates.Select((template, index) => new SubscriptionPlan
        {
            Id = nextPlanId++,
            CompanyId = company.Id,
            Name = template.Item1,
            BillingInterval = index == 3 ? "Annual" : "Monthly",
            MonthlyPrice = template.Item2 + faker.Random.Decimal(0m, 40m),
            SeatsIncluded = template.Item3,
            TrialDays = template.Item4,
            IsLegacy = index == 0 && faker.Random.Bool(0.18f)
        })).ToList();
    }

    private static List<CustomerSubscription> BuildCustomerSubscriptions(
        Faker faker,
        List<Customer> customers,
        List<SubscriptionPlan> subscriptionPlans,
        List<Project> projects,
        DateTime utcNow)
    {
        var nextSubscriptionId = 1;
        var subscriptions = new List<CustomerSubscription>();

        foreach (var customer in customers)
        {
            var companyPlans = subscriptionPlans.Where(x => x.CompanyId == customer.CompanyId).ToList();
            var companyProjects = projects.Where(x => x.CompanyId == customer.CompanyId).ToList();
            var plan = faker.PickRandom(companyPlans);
            var startedOn = DateOnly.FromDateTime(faker.Date.Past(3, utcNow));
            var status = faker.PickRandom<SubscriptionStatus>();
            DateOnly? cancelledOn = status == SubscriptionStatus.Cancelled
                ? startedOn.AddDays(faker.Random.Int(45, 360))
                : null;

            subscriptions.Add(new CustomerSubscription
            {
                Id = nextSubscriptionId++,
                CustomerId = customer.Id,
                SubscriptionPlanId = plan.Id,
                ProjectId = faker.Random.Bool(0.45f) ? faker.PickRandom(companyProjects).Id : null,
                StartedOn = startedOn,
                RenewsOn = status == SubscriptionStatus.Cancelled ? null : startedOn.AddDays(faker.Random.Int(30, 365)),
                CancelledOn = cancelledOn,
                Status = status,
                MonthlyRecurringRevenue = decimal.Round(plan.MonthlyPrice * faker.Random.Decimal(1m, 4m), 2),
                AutoRenew = status is SubscriptionStatus.Active or SubscriptionStatus.Trialing or SubscriptionStatus.PastDue
            });
        }

        return subscriptions;
    }

    private static List<SalesOpportunity> BuildSalesOpportunities(
        Faker faker,
        List<Customer> customers,
        List<Employee> employees,
        List<Project> projects,
        DateTime utcNow)
    {
        var nextOpportunityId = 1;
        var opportunities = new List<SalesOpportunity>();

        foreach (var companyId in customers.Select(x => x.CompanyId).Distinct())
        {
            var companyCustomers = customers.Where(x => x.CompanyId == companyId).ToList();
            var companyEmployees = employees.Where(x => x.CompanyId == companyId).ToList();
            var companyProjects = projects.Where(x => x.CompanyId == companyId).ToList();
            var opportunityCount = faker.Random.Int(MinimumOpportunitiesPerCompany, MaximumOpportunitiesPerCompany);

            for (var index = 0; index < opportunityCount; index++)
            {
                opportunities.Add(new SalesOpportunity
                {
                    Id = nextOpportunityId++,
                    CompanyId = companyId,
                    CustomerId = faker.PickRandom(companyCustomers).Id,
                    OwnerEmployeeId = faker.PickRandom(companyEmployees).Id,
                    ProjectId = faker.Random.Bool(0.35f) ? faker.PickRandom(companyProjects).Id : null,
                    Name = faker.Company.CatchPhrase(),
                    EstimatedValue = faker.Random.Decimal(8_000m, 180_000m),
                    Stage = faker.PickRandom<OpportunityStage>(),
                    ExpectedCloseOn = faker.Random.Bool(0.8f) ? DateOnly.FromDateTime(faker.Date.Soon(120, utcNow)) : null,
                    ConfidencePercent = (byte)faker.Random.Int(20, 95),
                    Source = faker.PickRandom(OpportunitySources),
                    CreatedAtUtc = faker.Date.Past(2, utcNow).ToUniversalTime()
                });
            }
        }

        return opportunities;
    }

    private static List<OpportunityNote> BuildOpportunityNotes(
        Faker faker,
        List<SalesOpportunity> opportunities,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextNoteId = 1;
        var notes = new List<OpportunityNote>();

        foreach (var opportunity in opportunities)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == opportunity.CompanyId).ToList();
            var noteCount = faker.Random.Int(1, 4);

            notes.AddRange(Enumerable.Range(0, noteCount).Select(index => new OpportunityNote
            {
                Id = nextNoteId++,
                OpportunityId = opportunity.Id,
                AuthorId = faker.PickRandom(companyEmployees).Id,
                Body = faker.Lorem.Sentences(2),
                CreatedAtUtc = faker.Date.Between(opportunity.CreatedAtUtc, utcNow).ToUniversalTime(),
                IsPinned = index == 0 && faker.Random.Bool(0.3f)
            }));
        }

        return notes;
    }

    private static List<SupportTicket> BuildSupportTickets(
        Faker faker,
        List<Customer> customers,
        List<Employee> employees,
        List<Project> projects,
        DateTime utcNow)
    {
        long nextTicketId = 200_000;
        var tickets = new List<SupportTicket>();

        foreach (var companyId in customers.Select(x => x.CompanyId).Distinct())
        {
            var companyCustomers = customers.Where(x => x.CompanyId == companyId).ToList();
            var companyEmployees = employees.Where(x => x.CompanyId == companyId).ToList();
            var companyProjects = projects.Where(x => x.CompanyId == companyId).ToList();
            var ticketCount = faker.Random.Int(MinimumTicketsPerCompany, MaximumTicketsPerCompany);

            for (var index = 0; index < ticketCount; index++)
            {
                var openedAt = faker.Date.Past(1, utcNow).ToUniversalTime();
                var status = faker.PickRandom<TicketStatus>();

                tickets.Add(new SupportTicket
                {
                    Id = nextTicketId++,
                    CompanyId = companyId,
                    CustomerId = faker.PickRandom(companyCustomers).Id,
                    ProjectId = faker.Random.Bool(0.4f) ? faker.PickRandom(companyProjects).Id : null,
                    AssignedEmployeeId = faker.Random.Bool(0.9f) ? faker.PickRandom(companyEmployees).Id : null,
                    Subject = faker.Hacker.Phrase(),
                    Description = faker.Lorem.Paragraphs(2),
                    Status = status,
                    Priority = faker.PickRandom<TicketPriority>(),
                    OpenedAtUtc = openedAt,
                    ResolvedAtUtc = status is TicketStatus.Resolved or TicketStatus.Closed
                        ? faker.Date.Between(openedAt, utcNow).ToUniversalTime()
                        : null,
                    SatisfactionScore = status == TicketStatus.Closed ? (byte?)faker.Random.Int(2, 5) : null
                });
            }
        }

        return tickets;
    }

    private static List<SupportTicketComment> BuildSupportTicketComments(
        Faker faker,
        List<SupportTicket> supportTickets,
        List<Employee> employees,
        List<CustomerContact> customerContacts,
        DateTime utcNow)
    {
        long nextCommentId = 300_000;
        var comments = new List<SupportTicketComment>();

        foreach (var ticket in supportTickets)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == ticket.CompanyId).ToList();
            var contacts = customerContacts.Where(x => x.CustomerId == ticket.CustomerId).ToList();
            var commentCount = faker.Random.Int(2, 6);

            for (var index = 0; index < commentCount; index++)
            {
                var employeeComment = index == 0 || faker.Random.Bool(0.65f);
                comments.Add(new SupportTicketComment
                {
                    Id = nextCommentId++,
                    SupportTicketId = ticket.Id,
                    AuthorEmployeeId = employeeComment ? faker.PickRandom(companyEmployees).Id : null,
                    CustomerContactId = employeeComment ? null : faker.PickRandom(contacts).Id,
                    Body = faker.Lorem.Sentences(2),
                    CreatedAtUtc = faker.Date.Between(ticket.OpenedAtUtc, utcNow).ToUniversalTime(),
                    IsInternal = employeeComment && faker.Random.Bool(0.25f)
                });
            }
        }

        return comments;
    }

    private static List<Vendor> BuildVendors(Faker faker, List<Company> companies)
    {
        var nextVendorId = 1;

        return companies.SelectMany(company => Enumerable.Range(0, VendorsPerCompany).Select(_ => new Vendor
        {
            Id = nextVendorId++,
            CompanyId = company.Id,
            Name = faker.Company.CompanyName(),
            Category = faker.PickRandom(VendorCategories),
            SupportEmail = faker.Internet.Email(provider: "vendor.efstudio.dev"),
            CountryCode = faker.PickRandom(OfficeLocations).CountryCode,
            Rating = decimal.Round(faker.Random.Decimal(2.5m, 5.0m), 2),
            IsPreferred = faker.Random.Bool(0.35f)
        })).ToList();
    }

    private static List<PurchaseOrder> BuildPurchaseOrders(
        Faker faker,
        List<Company> companies,
        List<Vendor> vendors,
        List<Employee> employees,
        List<Office> offices,
        DateTime utcNow)
    {
        var nextPurchaseOrderId = 1;
        var purchaseOrders = new List<PurchaseOrder>();

        foreach (var company in companies)
        {
            var companyVendors = vendors.Where(x => x.CompanyId == company.Id).ToList();
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var companyOffices = offices.Where(x => x.CompanyId == company.Id).ToList();
            var orderCount = faker.Random.Int(MinimumPurchaseOrdersPerCompany, MaximumPurchaseOrdersPerCompany);

            for (var index = 0; index < orderCount; index++)
            {
                var requester = faker.PickRandom(companyEmployees);
                var status = faker.PickRandom<PurchaseOrderStatus>();
                var orderedAt = faker.Date.Past(1, utcNow).ToUniversalTime();

                purchaseOrders.Add(new PurchaseOrder
                {
                    Id = nextPurchaseOrderId++,
                    CompanyId = company.Id,
                    VendorId = faker.PickRandom(companyVendors).Id,
                    RequestedByEmployeeId = requester.Id,
                    ApprovedByEmployeeId = status is PurchaseOrderStatus.Approved or PurchaseOrderStatus.Received
                        ? PickApproverId(faker, requester, companyEmployees)
                        : null,
                    OfficeId = faker.Random.Bool(0.7f) ? faker.PickRandom(companyOffices).Id : null,
                    OrderNumber = $"{company.Slug[..Math.Min(4, company.Slug.Length)].ToUpperInvariant()}-PO-{index + 1:D4}",
                    Status = status,
                    OrderedAtUtc = orderedAt,
                    ReceivedOn = status == PurchaseOrderStatus.Received
                        ? DateOnly.FromDateTime(faker.Date.Between(orderedAt, utcNow))
                        : null,
                    Currency = "USD",
                    Total = 0m,
                    ExternalReference = faker.Random.Bool(0.55f) ? faker.Random.AlphaNumeric(12).ToUpperInvariant() : null
                });
            }
        }

        return purchaseOrders;
    }

    private static List<PurchaseOrderLine> BuildPurchaseOrderLines(Faker faker, List<PurchaseOrder> purchaseOrders)
    {
        var nextLineId = 1;
        var lines = new List<PurchaseOrderLine>();

        foreach (var purchaseOrder in purchaseOrders)
        {
            var lineCount = faker.Random.Int(1, 5);
            decimal total = 0m;

            for (short sortOrder = 1; sortOrder <= lineCount; sortOrder++)
            {
                var quantity = faker.Random.Decimal(1m, 24m);
                var unitCost = faker.Random.Decimal(40m, 1_800m);
                var lineTotal = decimal.Round(quantity * unitCost, 2);
                total += lineTotal;

                lines.Add(new PurchaseOrderLine
                {
                    Id = nextLineId++,
                    PurchaseOrderId = purchaseOrder.Id,
                    Description = faker.Commerce.ProductName(),
                    Quantity = quantity,
                    UnitCost = unitCost,
                    LineTotal = lineTotal,
                    SortOrder = sortOrder
                });
            }

            purchaseOrder.Total = decimal.Round(total, 2);
        }

        return lines;
    }

    private static List<ExpenseClaim> BuildExpenseClaims(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        var nextClaimId = 1;
        var claims = new List<ExpenseClaim>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var claimCount = faker.Random.Int(MinimumExpenseClaimsPerCompany, MaximumExpenseClaimsPerCompany);

            for (var index = 0; index < claimCount; index++)
            {
                var employee = faker.PickRandom(companyEmployees);
                var status = faker.PickRandom<ExpenseStatus>();
                var submittedAt = faker.Date.Past(1, utcNow).ToUniversalTime();

                claims.Add(new ExpenseClaim
                {
                    Id = nextClaimId++,
                    EmployeeId = employee.Id,
                    ApproverId = status is ExpenseStatus.Approved or ExpenseStatus.Reimbursed
                        ? PickApproverId(faker, employee, companyEmployees)
                        : null,
                    Title = $"{faker.PickRandom(ExpenseCategories)} reimbursement",
                    Status = status,
                    SubmittedAtUtc = submittedAt,
                    ApprovedAtUtc = status is ExpenseStatus.Approved or ExpenseStatus.Reimbursed
                        ? faker.Date.Between(submittedAt, utcNow).ToUniversalTime()
                        : null,
                    Total = 0m,
                    Notes = faker.Random.Bool(0.4f) ? faker.Lorem.Sentence(12) : null
                });
            }
        }

        return claims;
    }

    private static List<ExpenseLine> BuildExpenseLines(Faker faker, List<ExpenseClaim> expenseClaims)
    {
        var nextLineId = 1;
        var lines = new List<ExpenseLine>();

        foreach (var claim in expenseClaims)
        {
            var lineCount = faker.Random.Int(1, 4);
            decimal total = 0m;

            for (var index = 0; index < lineCount; index++)
            {
                var amount = faker.Random.Decimal(18m, 480m);
                total += amount;

                lines.Add(new ExpenseLine
                {
                    Id = nextLineId++,
                    ExpenseClaimId = claim.Id,
                    OccurredOn = DateOnly.FromDateTime(faker.Date.Past(1)),
                    Category = faker.PickRandom(ExpenseCategories),
                    Merchant = faker.Company.CompanyName(),
                    Amount = amount,
                    IsBillable = faker.Random.Bool(0.35f),
                    ReceiptUrl = faker.Random.Bool(0.7f) ? $"https://receipts.example/{Guid.NewGuid():N}" : null
                });
            }

            claim.Total = decimal.Round(total, 2);
        }

        return lines;
    }

    private static List<Team> BuildTeams(Faker faker, List<Company> companies, List<Department> departments, DateTime utcNow)
    {
        var nextTeamId = 1;
        var teams = new List<Team>();

        foreach (var company in companies)
        {
            var companyDepartments = departments.Where(x => x.CompanyId == company.Id).ToList();

            for (var index = 0; index < TeamsPerCompany; index++)
            {
                var name = $"{faker.PickRandom(companyDepartments).Name} {faker.PickRandom(TeamFocusAreas)}";
                teams.Add(new Team
                {
                    Id = nextTeamId++,
                    CompanyId = company.Id,
                    DepartmentId = faker.Random.Bool(0.85f) ? faker.PickRandom(companyDepartments).Id : null,
                    Name = name,
                    Slug = $"{ToSlug(name)}-{index + 1}",
                    FocusArea = faker.PickRandom(TeamFocusAreas),
                    CreatedAtUtc = faker.Date.Past(4, utcNow).ToUniversalTime(),
                    IsActive = faker.Random.Bool(0.92f)
                });
            }
        }

        return teams;
    }

    private static List<TeamMembership> BuildTeamMemberships(Faker faker, List<Team> teams, List<Employee> employees, DateTime utcNow)
    {
        var nextMembershipId = 1;
        var memberships = new List<TeamMembership>();

        foreach (var team in teams)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == team.CompanyId).ToList();
            var memberCount = faker.Random.Int(4, 9);
            var selectedEmployees = companyEmployees.OrderBy(_ => faker.Random.Int()).Take(memberCount).ToList();

            memberships.AddRange(selectedEmployees.Select((employee, index) => new TeamMembership
            {
                Id = nextMembershipId++,
                TeamId = team.Id,
                EmployeeId = employee.Id,
                Role = index == 0 ? "Lead" : faker.PickRandom("Member", "Contributor", "Advisor"),
                JoinedAtUtc = faker.Date.Past(3, utcNow).ToUniversalTime(),
                IsLead = index == 0
            }));
        }

        return memberships;
    }

    private static List<TrainingCourse> BuildTrainingCourses(Faker faker, List<Company> companies, List<Department> departments, DateTime utcNow)
    {
        var nextCourseId = 1;
        var courses = new List<TrainingCourse>();

        foreach (var company in companies)
        {
            var companyDepartments = departments.Where(x => x.CompanyId == company.Id).ToList();

            for (var index = 0; index < CoursesPerCompany; index++)
            {
                courses.Add(new TrainingCourse
                {
                    Id = nextCourseId++,
                    CompanyId = company.Id,
                    DepartmentId = faker.Random.Bool(0.8f) ? faker.PickRandom(companyDepartments).Id : null,
                    Title = faker.Company.Bs(),
                    DeliveryMethod = faker.PickRandom(CourseDeliveryMethods),
                    DurationHours = faker.Random.Decimal(1m, 16m),
                    PublishedAtUtc = faker.Date.Past(3, utcNow).ToUniversalTime(),
                    IsRequired = faker.Random.Bool(0.35f)
                });
            }
        }

        return courses;
    }

    private static List<CourseEnrollment> BuildCourseEnrollments(
        Faker faker,
        List<TrainingCourse> trainingCourses,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextEnrollmentId = 1;
        var enrollments = new List<CourseEnrollment>();

        foreach (var course in trainingCourses)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == course.CompanyId).ToList();
            var selectedEmployees = companyEmployees.OrderBy(_ => faker.Random.Int()).Take(faker.Random.Int(5, 12)).ToList();

            enrollments.AddRange(selectedEmployees.Select(employee =>
            {
                var enrolledAt = faker.Date.Between(course.PublishedAtUtc, utcNow).ToUniversalTime();
                var status = faker.PickRandom<EnrollmentStatus>();
                return new CourseEnrollment
                {
                    Id = nextEnrollmentId++,
                    TrainingCourseId = course.Id,
                    EmployeeId = employee.Id,
                    EnrolledAtUtc = enrolledAt,
                    CompletedAtUtc = status == EnrollmentStatus.Completed
                        ? faker.Date.Between(enrolledAt, utcNow).ToUniversalTime()
                        : null,
                    Status = status,
                    Score = status == EnrollmentStatus.Completed ? decimal.Round(faker.Random.Decimal(70m, 100m), 2) : null
                };
            }));
        }

        return enrollments;
    }

    private static List<KnowledgeBaseArticle> BuildKnowledgeBaseArticles(
        Faker faker,
        List<Company> companies,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextArticleId = 1;
        var articles = new List<KnowledgeBaseArticle>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();

            for (var index = 0; index < ArticlesPerCompany; index++)
            {
                var title = faker.Lorem.Sentence(5);
                articles.Add(new KnowledgeBaseArticle
                {
                    Id = nextArticleId++,
                    CompanyId = company.Id,
                    AuthorId = faker.PickRandom(companyEmployees).Id,
                    Title = title,
                    Slug = $"{ToSlug(title)}-{index + 1}",
                    Summary = faker.Lorem.Sentence(12),
                    Body = faker.Lorem.Paragraphs(3),
                    PublishedAtUtc = faker.Random.Bool(0.8f) ? faker.Date.Past(2, utcNow).ToUniversalTime() : null,
                    ViewCount = faker.Random.Int(0, 12_000),
                    IsArchived = faker.Random.Bool(0.08f)
                });
            }
        }

        return articles;
    }

    private static List<CompanyEvent> BuildCompanyEvents(
        Faker faker,
        List<Company> companies,
        List<Office> offices,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextEventId = 1;
        var events = new List<CompanyEvent>();

        foreach (var company in companies)
        {
            var companyOffices = offices.Where(x => x.CompanyId == company.Id).ToList();
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();

            for (var index = 0; index < EventsPerCompany; index++)
            {
                var startsAt = new DateTimeOffset(faker.Date.Soon(120, utcNow), TimeSpan.Zero);
                events.Add(new CompanyEvent
                {
                    Id = nextEventId++,
                    CompanyId = company.Id,
                    OfficeId = faker.Random.Bool(0.75f) ? faker.PickRandom(companyOffices).Id : null,
                    HostEmployeeId = faker.Random.Bool(0.85f) ? faker.PickRandom(companyEmployees).Id : null,
                    Name = faker.Company.CatchPhrase(),
                    EventType = faker.PickRandom(EventTypes),
                    StartsAtUtc = startsAt,
                    EndsAtUtc = startsAt.AddHours(faker.Random.Int(1, 8)),
                    Capacity = (short)faker.Random.Int(25, 180),
                    IsVirtual = faker.Random.Bool(0.3f)
                });
            }
        }

        return events;
    }

    private static List<EventAttendance> BuildEventAttendances(
        Faker faker,
        List<CompanyEvent> companyEvents,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextAttendanceId = 1;
        var attendances = new List<EventAttendance>();

        foreach (var companyEvent in companyEvents)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == companyEvent.CompanyId).ToList();
            var selectedEmployees = companyEmployees.OrderBy(_ => faker.Random.Int()).Take(faker.Random.Int(8, 18)).ToList();

            attendances.AddRange(selectedEmployees.Select(employee =>
            {
                var registeredAt = faker.Date.Past(1, utcNow).ToUniversalTime();
                var status = faker.PickRandom<AttendanceStatus>();
                return new EventAttendance
                {
                    Id = nextAttendanceId++,
                    CompanyEventId = companyEvent.Id,
                    EmployeeId = employee.Id,
                    RegisteredAtUtc = registeredAt,
                    CheckedInAtUtc = status == AttendanceStatus.Attended
                        ? companyEvent.StartsAtUtc.AddMinutes(faker.Random.Int(-10, 20))
                        : null,
                    Status = status
                };
            }));
        }

        return attendances;
    }

    private static List<IntegrationEndpoint> BuildIntegrationEndpoints(Faker faker, List<Company> companies, DateTime utcNow)
    {
        var nextEndpointId = 1;
        var endpoints = new List<IntegrationEndpoint>();

        foreach (var company in companies)
        {
            foreach (var provider in IntegrationProviders.Take(EndpointsPerCompany))
            {
                endpoints.Add(new IntegrationEndpoint
                {
                    Id = nextEndpointId++,
                    CompanyId = company.Id,
                    Name = $"{provider} workspace",
                    Provider = provider,
                    BaseUrl = $"https://api.{provider.ToLowerInvariant()}.sample.efstudio.dev/{company.Slug}",
                    IsEnabled = faker.Random.Bool(0.9f),
                    LastSyncedAtUtc = faker.Random.Bool(0.75f) ? faker.Date.RecentOffset(14, utcNow).ToUniversalTime() : null
                });
            }
        }

        return endpoints;
    }

    private static List<ApiCredential> BuildApiCredentials(
        Faker faker,
        List<IntegrationEndpoint> integrationEndpoints,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextCredentialId = 1;
        var credentials = new List<ApiCredential>();

        foreach (var endpoint in integrationEndpoints)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == endpoint.CompanyId).ToList();
            var credentialCount = faker.Random.Int(1, 3);

            credentials.AddRange(Enumerable.Range(0, credentialCount).Select(index => new ApiCredential
            {
                Id = nextCredentialId++,
                CompanyId = endpoint.CompanyId,
                IntegrationEndpointId = endpoint.Id,
                OwnerEmployeeId = faker.Random.Bool(0.7f) ? faker.PickRandom(companyEmployees).Id : null,
                Name = index == 0 ? "Primary token" : $"Automation token {index}",
                Provider = Enum.TryParse<CredentialProvider>(endpoint.Provider, out var provider) ? provider : CredentialProvider.GitHub,
                KeyPrefix = faker.Random.String2(6, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"),
                ExpiresAtUtc = faker.Random.Bool(0.45f) ? faker.Date.FutureOffset(1, utcNow).ToUniversalTime() : null,
                LastUsedAtUtc = faker.Random.Bool(0.65f) ? faker.Date.RecentOffset(45, utcNow).ToUniversalTime() : null,
                IsRevoked = faker.Random.Bool(0.08f)
            }));
        }

        return credentials;
    }

    private static List<ReleaseNote> BuildReleaseNotes(
        Faker faker,
        List<Project> projects,
        List<ProjectMembership> memberships,
        DateTime utcNow)
    {
        var nextReleaseNoteId = 1;
        var releaseNotes = new List<ReleaseNote>();

        foreach (var project in projects)
        {
            var memberIds = memberships.Where(x => x.ProjectId == project.Id).Select(x => x.EmployeeId).ToList();
            var releaseCount = faker.Random.Int(1, 3);

            for (var index = 0; index < releaseCount; index++)
            {
                releaseNotes.Add(new ReleaseNote
                {
                    Id = nextReleaseNoteId++,
                    ProjectId = project.Id,
                    AuthorId = faker.PickRandom(memberIds),
                    Version = $"{faker.Random.Int(1, 4)}.{faker.Random.Int(0, 12)}.{faker.Random.Int(0, 24)}",
                    Title = faker.Lorem.Sentence(6),
                    Highlights = faker.Lorem.Paragraphs(2),
                    ReleasedAtUtc = faker.Date.Past(1, utcNow).ToUniversalTime(),
                    Kind = faker.PickRandom<ReleaseKind>(),
                    IsBreakingChange = faker.Random.Bool(0.12f)
                });
            }
        }

        return releaseNotes;
    }

    private static List<FeatureFlag> BuildFeatureFlags(Faker faker, List<Project> projects, DateTime utcNow)
    {
        var nextFeatureFlagId = 1;
        var featureFlags = new List<FeatureFlag>();

        foreach (var project in projects)
        {
            var flagCount = faker.Random.Int(2, 5);
            for (var index = 0; index < flagCount; index++)
            {
                featureFlags.Add(new FeatureFlag
                {
                    Id = nextFeatureFlagId++,
                    ProjectId = project.Id,
                    Name = $"{ToSlug(faker.Hacker.Noun())}-{index + 1}",
                    FlagType = faker.PickRandom<FeatureFlagType>(),
                    RolloutPercentage = (byte)faker.Random.Int(0, 100),
                    IsEnabled = faker.Random.Bool(0.7f),
                    CreatedAtUtc = faker.Date.Past(1, utcNow).ToUniversalTime(),
                    ExpiresAtUtc = faker.Random.Bool(0.18f) ? faker.Date.Future(1, utcNow).ToUniversalTime() : null
                });
            }
        }

        return featureFlags;
    }

    private static List<Sprint> BuildSprints(Faker faker, List<Project> projects, DateTime utcNow)
    {
        var nextSprintId = 1;
        var sprints = new List<Sprint>();

        foreach (var project in projects)
        {
            var sprintCount = faker.Random.Int(2, 4);
            for (var index = 0; index < sprintCount; index++)
            {
                var startsOn = DateOnly.FromDateTime(utcNow.Date.AddDays(-(index + 1) * 14));
                sprints.Add(new Sprint
                {
                    Id = nextSprintId++,
                    ProjectId = project.Id,
                    Name = $"Sprint {index + 1}",
                    Goal = faker.Lorem.Sentence(10),
                    StartsOn = startsOn,
                    EndsOn = startsOn.AddDays(13),
                    GoalStatus = faker.PickRandom<SprintGoalStatus>(),
                    CapacityHours = faker.Random.Decimal(80m, 320m)
                });
            }
        }

        return sprints;
    }

    private static List<SprintAssignment> BuildSprintAssignments(
        Faker faker,
        List<Sprint> sprints,
        List<WorkItem> workItems,
        DateTime utcNow)
    {
        var nextAssignmentId = 1;
        var assignments = new List<SprintAssignment>();

        foreach (var sprint in sprints)
        {
            var projectWorkItems = workItems.Where(x => x.ProjectId == sprint.ProjectId).ToList();
            var selectedWorkItems = projectWorkItems.OrderBy(_ => faker.Random.Int()).Take(faker.Random.Int(4, 8)).ToList();

            assignments.AddRange(selectedWorkItems.Select((workItem, index) => new SprintAssignment
            {
                Id = nextAssignmentId++,
                SprintId = sprint.Id,
                WorkItemId = workItem.Id,
                AssignedAtUtc = faker.Date.Between(sprint.StartsOn.ToDateTime(TimeOnly.MinValue), utcNow).ToUniversalTime(),
                SortOrder = (short)(index + 1)
            }));
        }

        return assignments;
    }

    private static List<TimeEntry> BuildTimeEntries(
        Faker faker,
        List<ProjectMembership> memberships,
        List<WorkItem> workItems,
        DateTime utcNow)
    {
        long nextTimeEntryId = 400_000;
        var timeEntries = new List<TimeEntry>();

        foreach (var membership in memberships)
        {
            var projectWorkItems = workItems.Where(x => x.ProjectId == membership.ProjectId).ToList();
            var entryCount = faker.Random.Int(2, 6);

            for (var index = 0; index < entryCount; index++)
            {
                var loggedOn = DateOnly.FromDateTime(faker.Date.Recent(60, utcNow));
                timeEntries.Add(new TimeEntry
                {
                    Id = nextTimeEntryId++,
                    EmployeeId = membership.EmployeeId,
                    ProjectId = membership.ProjectId,
                    WorkItemId = faker.Random.Bool(0.8f) ? faker.PickRandom(projectWorkItems).Id : null,
                    LoggedOn = loggedOn,
                    Hours = decimal.Round(faker.Random.Decimal(0.5m, 8m), 2),
                    Description = faker.Hacker.Phrase(),
                    Source = faker.PickRandom<TimeEntrySource>(),
                    CreatedAtUtc = loggedOn.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                });
            }
        }

        return timeEntries;
    }

    private static List<BenefitPlan> BuildBenefitPlans(Faker faker, List<Company> companies)
    {
        var nextPlanId = 1;
        var planTemplates = new[]
        {
            ("Medical PPO", "Aetna", "Medical"),
            ("Dental Basic", "Delta Dental", "Dental"),
            ("Vision Plus", "VSP", "Vision")
        };

        return companies.SelectMany(company => planTemplates.Select(template => new BenefitPlan
        {
            Id = nextPlanId++,
            CompanyId = company.Id,
            Name = template.Item1,
            Provider = template.Item2,
            PlanType = template.Item3,
            EmployeeMonthlyCost = faker.Random.Decimal(25m, 180m),
            EmployerMonthlyContribution = faker.Random.Decimal(120m, 780m),
            IsActive = faker.Random.Bool(0.9f)
        })).ToList();
    }

    private static List<EmployeeBenefitEnrollment> BuildBenefitEnrollments(
        Faker faker,
        List<BenefitPlan> benefitPlans,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextEnrollmentId = 1;
        var enrollments = new List<EmployeeBenefitEnrollment>();

        foreach (var companyId in benefitPlans.Select(x => x.CompanyId).Distinct())
        {
            var companyPlans = benefitPlans.Where(x => x.CompanyId == companyId).ToList();
            var selectedEmployees = employees.Where(x => x.CompanyId == companyId).OrderBy(_ => faker.Random.Int()).Take(12).ToList();

            enrollments.AddRange(selectedEmployees.Select(employee =>
            {
                var plan = faker.PickRandom(companyPlans);
                var effectiveOn = DateOnly.FromDateTime(faker.Date.Past(2, utcNow));
                return new EmployeeBenefitEnrollment
                {
                    Id = nextEnrollmentId++,
                    BenefitPlanId = plan.Id,
                    EmployeeId = employee.Id,
                    EffectiveOn = effectiveOn,
                    EndedOn = faker.Random.Bool(0.08f) ? effectiveOn.AddDays(faker.Random.Int(45, 240)) : null,
                    CoverageLevel = faker.PickRandom(BenefitCoverageLevels),
                    IsPrimary = true
                };
            }));
        }

        return enrollments;
    }

    private static List<LeaveRequest> BuildLeaveRequests(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        var nextLeaveRequestId = 1;
        var leaveRequests = new List<LeaveRequest>();

        foreach (var company in companies)
        {
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var requestCount = faker.Random.Int(10, 18);

            for (var index = 0; index < requestCount; index++)
            {
                var employee = faker.PickRandom(companyEmployees);
                var startsOn = DateOnly.FromDateTime(faker.Date.Soon(120, utcNow));
                var status = faker.PickRandom<LeaveRequestStatus>();

                leaveRequests.Add(new LeaveRequest
                {
                    Id = nextLeaveRequestId++,
                    EmployeeId = employee.Id,
                    ApproverId = status == LeaveRequestStatus.Approved
                        ? PickApproverId(faker, employee, companyEmployees)
                        : null,
                    LeaveType = faker.PickRandom(LeaveTypes),
                    StartsOn = startsOn,
                    EndsOn = startsOn.AddDays(faker.Random.Int(0, 10)),
                    Status = status,
                    RequestedAtUtc = faker.Date.Past(1, utcNow).ToUniversalTime(),
                    Reason = faker.Random.Bool(0.7f) ? faker.Lorem.Sentence(8) : null,
                    IsHalfDay = faker.Random.Bool(0.1f)
                });
            }
        }

        return leaveRequests;
    }

    private static List<Candidate> BuildCandidates(
        Faker faker,
        List<Company> companies,
        List<Department> departments,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextCandidateId = 1;
        var candidates = new List<Candidate>();

        foreach (var company in companies)
        {
            var companyDepartments = departments.Where(x => x.CompanyId == company.Id).ToList();
            var companyEmployees = employees.Where(x => x.CompanyId == company.Id).ToList();
            var candidateCount = faker.Random.Int(MinimumCandidatesPerCompany, MaximumCandidatesPerCompany);

            for (var index = 0; index < candidateCount; index++)
            {
                var fullName = faker.Name.FullName();
                candidates.Add(new Candidate
                {
                    Id = nextCandidateId++,
                    CompanyId = company.Id,
                    DepartmentId = faker.Random.Bool(0.9f) ? faker.PickRandom(companyDepartments).Id : null,
                    RecruiterEmployeeId = faker.Random.Bool(0.85f) ? faker.PickRandom(companyEmployees).Id : null,
                    FullName = fullName,
                    Email = faker.Internet.Email(firstName: fullName.Split(' ')[0], lastName: fullName.Split(' ').Last(), provider: "candidate.efstudio.dev").ToLowerInvariant(),
                    Stage = faker.PickRandom<CandidateStage>(),
                    Source = faker.PickRandom(CandidateSources),
                    AppliedOn = DateOnly.FromDateTime(faker.Date.Past(1, utcNow)),
                    DesiredSalary = faker.Random.Bool(0.6f) ? faker.Random.Decimal(60_000m, 190_000m) : null,
                    IsRemote = faker.Random.Bool(0.45f)
                });
            }
        }

        return candidates;
    }

    private static List<InterviewSession> BuildInterviewSessions(
        Faker faker,
        List<Candidate> candidates,
        List<Employee> employees,
        DateTime utcNow)
    {
        var nextInterviewId = 1;
        var interviews = new List<InterviewSession>();

        foreach (var candidate in candidates.Where(x => x.Stage is CandidateStage.Screening or CandidateStage.Interviewing or CandidateStage.Offer))
        {
            var companyEmployees = employees.Where(x => x.CompanyId == candidate.CompanyId).ToList();
            var sessionCount = faker.Random.Int(1, 3);

            interviews.AddRange(Enumerable.Range(0, sessionCount).Select(_ => new InterviewSession
            {
                Id = nextInterviewId++,
                CandidateId = candidate.Id,
                InterviewerEmployeeId = faker.PickRandom(companyEmployees).Id,
                ScheduledAtUtc = faker.Date.SoonOffset(45, utcNow),
                DurationMinutes = (short)faker.Random.Int(30, 90),
                Format = faker.PickRandom<InterviewFormat>(),
                Score = faker.Random.Bool(0.45f) ? decimal.Round(faker.Random.Decimal(2.5m, 5.0m), 2) : null,
                Notes = faker.Random.Bool(0.55f) ? faker.Lorem.Sentence(10) : null
            }));
        }

        return interviews;
    }

    private static List<AuditLog> BuildAuditLogs(Faker faker, List<Company> companies, List<Employee> employees, DateTime utcNow)
    {
        long nextAuditLogId = 900_000;
        string[] entityNames =
        [
            "Project", "WorkItem", "Invoice", "Employee", "Asset", "Post", "Customer", "SupportTicket",
            "PurchaseOrder", "ExpenseClaim", "ReleaseNote", "Candidate", "FeatureFlag", "Sprint"
        ];
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
                EntityId = faker.Random.Int(1, 25_000).ToString(),
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

    private static Guid PickApproverId(Faker faker, Employee employee, List<Employee> companyEmployees)
    {
        if (employee.ManagerId is Guid managerId)
        {
            return managerId;
        }

        return faker.PickRandom(companyEmployees.Where(x => x.Id != employee.Id).ToList()).Id;
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
