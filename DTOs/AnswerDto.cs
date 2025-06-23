namespace UniHelp.Api.DTOs;

/// <summary>
/// Bir cevabı, yazar bilgisiyle birlikte istemciye göstermek için kullanılır.
/// </summary>
public class AnswerDto
{
    /// <summary>
    /// Cevabın benzersiz kimliği (ID).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Cevabın metni.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Cevabın oluşturulma tarihi ve saati (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Cevabı yazan kullanıcının adı.
    /// </summary>
    public string AuthorUsername { get; set; } = string.Empty;
}