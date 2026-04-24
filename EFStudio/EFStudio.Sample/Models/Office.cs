namespace EFStudio.Sample.Sqlite.Models;

public class Office
{
    public Guid Id { get; set; }
    public int CompanyId { get; set; }
    public required string Name { get; set; }
    public required string CountryCode { get; set; }
    public required string TimeZone { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public short FloorCount { get; set; }
    public bool IsHeadquarters { get; set; }
    public Company? Company { get; set; }
    public List<Employee> Employees { get; set; } = [];
}
