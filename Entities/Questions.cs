using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.Entities;

public class Question
{
    public int Id { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public int UserId { get; set; }

    // Navigation Properties
    public  User User { get; set; } = null!;
    public  ICollection<Answer> Answers { get; set; } = new List<Answer>();  
}