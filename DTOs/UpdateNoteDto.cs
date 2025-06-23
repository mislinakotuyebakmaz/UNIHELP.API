using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

/// <summary>
/// Mevcut bir notu güncellemek için istemciden alınan verileri taşır.
/// </summary>
public class UpdateNoteDto
{
    /// <summary>
    /// Notun yeni başlığı.
    /// </summary>
    /// <example>Ders 5 Konularının Güncel Hali</example>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notun yeni metin içeriği.
    /// </summary>
    /// <example>DI konusu daha detaylı işlendi ve örnekler eklendi.</example>
    public string? Content { get; set; }

    /// <summary>
    /// Nota eklenecek yeni dosyanın URL'si (varsa).
    /// </summary>
    /// <example>http://example.com/notlar/ders5_v2.pdf</example>
    public string? FileUrl { get; set; }
}