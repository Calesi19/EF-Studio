namespace EFStudio.Sample.Sqlite.Models;

public class IntegrationEndpoint
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public required string Name { get; set; }
    public required string Provider { get; set; }
    public required string BaseUrl { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? LastSyncedAtUtc { get; set; }
    public Company? Company { get; set; }
    public List<ApiCredential> ApiCredentials { get; set; } = [];
}
