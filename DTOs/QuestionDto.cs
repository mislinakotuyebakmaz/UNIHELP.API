namespace UniHelp.Api.DTOs;

/// <summary>
/// Bir soruyu, yazar bilgisi ve ona ait tüm cevaplarla birlikte istemciye göstermek için kullanılır.
/// </summary>
public class QuestionDto
{
    /// <summary>
    /// Sorunun benzersiz kimliği (ID).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Sorunun başlığı.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Sorunun detaylı açıklaması / metni.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Sorunun oluşturulma tarihi ve saati (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Soruyu oluşturan kullanıcının adı.
    /// </summary>
    public string AuthorUsername { get; set; } = string.Empty;

    /// <summary>
    /// Bu soruya verilmiş olan tüm cevapların bir listesi.
    /// Her bir cevap, AnswerDto formatındadır.
    /// </summary>
    public ICollection<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
}