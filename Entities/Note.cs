using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.Entities;

public class Note
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; } // İçerik boş olabilir

    public string? FileUrl { get; set; } // Yüklenen dosyanın linki

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key (İlişki için anahtar)
    public int UserId { get; set; }

    // Navigation Property (İlişki)
   public  User User { get; set; } = null!;
}