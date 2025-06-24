using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

/// <summary>
/// Yeni bir soru oluşturmak için istemciden alınan verileri taşır.
/// </summary>
public class CreateQuestionDto
{
    /// <summary>
    /// Sorunun kısa ve açıklayıcı başlığı.
    /// </summary>
    /// <example>ASP.NET Core'da SignalR nasıl kullanılır?</example>
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Sorunun detaylı açıklaması ve kod örnekleri (varsa).
    /// </summary>
    /// <example>Bir kullanıcı başka bir kullanıcıya bildirim göndermek istediğinde, hangi adımları izlemeliyim?</example>
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Body { get; set; } = string.Empty;
}