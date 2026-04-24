namespace EFStudio.Sample.Sqlite.Models;

public class FeatureFlag
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public required string Name { get; set; }
    public FeatureFlagType FlagType { get; set; }
    public byte RolloutPercentage { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public Project? Project { get; set; }
}
