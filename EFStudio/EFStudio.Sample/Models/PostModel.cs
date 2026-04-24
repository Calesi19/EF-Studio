namespace EFStudio.Sample.Sqlite.Models;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public List<Post> Posts { get; set; } = []; // Using C# 12+ collection expression
}

public class Post
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Content { get; set; }
    public int UserId { get; set; }
    public User? Author { get; set; }
}
