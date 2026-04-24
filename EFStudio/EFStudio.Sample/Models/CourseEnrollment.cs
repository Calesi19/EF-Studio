namespace EFStudio.Sample.Sqlite.Models;

public class CourseEnrollment
{
    public int Id { get; set; }
    public int TrainingCourseId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime EnrolledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public EnrollmentStatus Status { get; set; }
    public decimal? Score { get; set; }
    public TrainingCourse? TrainingCourse { get; set; }
    public Employee? Employee { get; set; }
}
