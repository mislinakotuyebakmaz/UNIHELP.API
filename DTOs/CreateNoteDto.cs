using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

/// <summary>
/// Yeni bir not oluşturmak için istemciden alınan verileri taşır.
/// </summary>
public class CreateNoteDto
{
    /// <summary>
    /// Oluşturulacak notun başlığı.
    /// </summary>
    /// <example>Ders 5 Önemli Konular</example>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notun metin içeriği.
    /// </summary>
    /// <example>Bu derste Dependency Injection konusu işlendi...</example>
    public string? Content { get; set; }

    /// <summary>
    /// Nota eklenecek dosyanın URL'si (varsa).
    /// </summary>
    /// <example>http://example.com/notlar/ders5.pdf</example>
    public string? FileUrl { get; set; }
}