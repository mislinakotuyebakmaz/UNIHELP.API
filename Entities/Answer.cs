namespace UniHelp.Api.Entities;

public class Answer
{
    public int Id { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int UserId { get; set; }
    public int QuestionId { get; set; }

    // Navigation Properties
    public  User User { get; set; } = null!;
    public  Question Question { get; set; } = null!;
}