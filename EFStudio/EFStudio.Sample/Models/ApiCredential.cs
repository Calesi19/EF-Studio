namespace EFStudio.Sample.Sqlite.Models;

public class ApiCredential
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int IntegrationEndpointId { get; set; }
    public Guid? OwnerEmployeeId { get; set; }
    public required string Name { get; set; }
    public CredentialProvider Provider { get; set; }
    public required string KeyPrefix { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public DateTimeOffset? LastUsedAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public Company? Company { get; set; }
    public IntegrationEndpoint? IntegrationEndpoint { get; set; }
    public Employee? OwnerEmployee { get; set; }
}
