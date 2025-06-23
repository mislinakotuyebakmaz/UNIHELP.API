namespace UniHelp.Api.DTOs;

/// <summary>
/// Bir notu, yazar bilgisiyle birlikte istemciye göstermek için kullanılır.
/// </summary>
public class NoteDto
{
    /// <summary>
    /// Notun benzersiz kimliği (ID).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Notun başlığı.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notun metin içeriği. Boş olabilir.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Nota eklenmiş bir dosyanın URL'si. Boş olabilir.
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// Notun oluşturulma tarihi ve saati (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Notu oluşturan kullanıcının adı.
    /// </summary>
    public string AuthorUsername { get; set; } = string.Empty;
}