using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

/// <summary>
/// Mevcut bir soruya yeni bir cevap eklemek için istemciden alınan verileri taşır.
/// </summary>
public class CreateAnswerDto
{
    /// <summary>
    /// Cevabın metin içeriği.
    /// </summary>
    /// <example>Öncelikle bir SignalR Hub'ı oluşturman gerekiyor...</example>
    [Required]
    public string Body { get; set; } = string.Empty;
}