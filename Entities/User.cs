using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public byte[] PasswordHash { get; set; } = [];

    [Required]
    public byte[] PasswordSalt { get; set; } = [];

    // Navigation Properties (İlişkiler)
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}