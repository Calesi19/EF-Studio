namespace EFStudio.Sample.Sqlite.Models;

public class Person
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public List<Post> Posts { get; set; } = []; // Using C# 12+ collection expression
}
