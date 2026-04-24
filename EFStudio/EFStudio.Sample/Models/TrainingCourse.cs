namespace EFStudio.Sample.Sqlite.Models;

public class TrainingCourse
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public required string Title { get; set; }
    public required string DeliveryMethod { get; set; }
    public decimal DurationHours { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public bool IsRequired { get; set; }
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public List<CourseEnrollment> Enrollments { get; set; } = [];
}
