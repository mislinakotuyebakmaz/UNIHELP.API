using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;
using UniHelp.Api.Hubs;

namespace UniHelp.Api.Controllers;

/// <summary>
/// Belirli bir soruya ait cevapları yönetmek için kullanılan endpoint'leri içerir.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/questions/{questionId}/[controller]")]
public class AnswersController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<AnswersController> _logger;

    /// <summary>
    /// AnswersController için gerekli servisleri enjekte eder.
    /// </summary>
    /// <param name="context">Veritabanı işlemleri için DataContext.</param>
    /// <param name="hubContext">Gerçek zamanlı bildirimler için SignalR HubContext.</param>
    /// <param name="logger">Loglama işlemleri için Logger.</param>
    public AnswersController(
        DataContext context, 
        IHubContext<NotificationHub> hubContext, 
        ILogger<AnswersController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen bir soruya yeni bir cevap ekler.
    /// </summary>
    /// <remarks>
    /// Cevap eklendiğinde, sorunun sahibine gerçek zamanlı bir bildirim gönderir (eğer cevap yazan kişi sorunun sahibi değilse).
    /// </remarks>
    /// <param name="questionId">Cevap eklenecek sorunun ID'si.</param>
    /// <param name="createAnswerDto">Yeni cevabın içeriğini taşıyan DTO.</param>
    /// <returns>Oluşturulan cevabın bilgilerini içeren bir AnswerDto.</returns>
    /// <response code="200">Cevap başarıyla oluşturulduğunda döner.</response>
    /// <response code="401">Kullanıcı giriş yapmamışsa döner.</response>
    /// <response code="404">Belirtilen ID'ye sahip soru bulunamazsa döner.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AnswerDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AnswerDto>> CreateAnswer(int questionId, CreateAnswerDto createAnswerDto)
    {
        // Cevap yazan kullanıcının bilgilerini alıyoruz
        var answeringUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (answeringUserIdString == null) return Unauthorized();
        var answeringUserId = int.Parse(answeringUserIdString);

        var user = await _context.Users.FindAsync(answeringUserId);
        if (user == null) return Unauthorized();

        // Cevap yazılan soruyu ve sahibini buluyoruz
        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionId);
        if (question == null) return NotFound("Question not found.");
        
        var questionOwnerIdString = question.UserId.ToString();

        // Veritabanına yeni cevabı kaydediyoruz
        var newAnswer = new Answer
        {
            Body = createAnswerDto.Body,
            CreatedAt = DateTime.UtcNow,
            UserId = answeringUserId,
            QuestionId = questionId
        };
        _context.Answers.Add(newAnswer);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Yeni cevap (ID: {AnswerId}) veritabanına kaydedildi.", newAnswer.Id);

        // --- BİLDİRİM GÖNDERME ve LOGLAMA MANTIĞI ---

        // Sadece, cevap yazan kişi sorunun sahibi değilse bildirim gönderiyoruz
        if (questionOwnerIdString != answeringUserIdString)
        {
            var message = $"{user.Username} kullanıcısı, '{question.Title}' başlıklı sorunuza bir cevap yazdı.";
            var groupName = $"user_{questionOwnerIdString}";

            _logger.LogInformation("--> Bildirim gönderilmeye çalışılıyor...");
            _logger.LogInformation("--> Hedef Grup Adı: {GroupName}", groupName);
            _logger.LogInformation("--> Gönderilecek Mesaj: {Message}", message);
            
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", message);
                _logger.LogInformation("--> Bildirim başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "--> Bildirim gönderilirken bir HATA oluştu!");
            }
        }
        else
        {
            _logger.LogInformation("--> Kullanıcı kendi sorusuna cevap yazdığı için bildirim gönderilmedi.");
        }
        // ------------------------------------

        // Kullanıcıya DTO olarak yanıtı döndürüyoruz
        var answerToReturn = new AnswerDto
        {
            Id = newAnswer.Id,
            Body = newAnswer.Body,
            CreatedAt = newAnswer.CreatedAt,
            AuthorUsername = user.Username
        };
        return Ok(answerToReturn);
    }

    /// <summary>
    /// SignalR bildirimlerini test etmek için kullanılır.
    /// </summary>
    /// <returns>Test sonucu.</returns>
    [HttpPost("test-notification")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> TestNotification()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) return Unauthorized();
        
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var groupName = $"user_{userIdString}";
        var message = $"Test bildirimi - Kullanıcı: {username}, Saat: {DateTime.Now:HH:mm:ss}";

        _logger.LogInformation("🔔 Test bildirimi gönderiliyor...");
        _logger.LogInformation("👤 Hedef Kullanıcı: {Username} (ID: {UserId})", username, userIdString);
        _logger.LogInformation("🎯 Hedef Grup: {GroupName}", groupName);
        _logger.LogInformation("💬 Mesaj: {Message}", message);

        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", message);
            _logger.LogInformation("✅ Test bildirimi başarıyla gönderildi!");
            
            return Ok(new { 
                success = true, 
                message = "Test bildirimi gönderildi",
                targetUser = username,
                targetGroup = groupName,
                sentMessage = message,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Test bildirimi gönderilirken hata oluştu!");
            return Ok(new { 
                success = false, 
                message = "Test bildirimi gönderilemedi",
                error = ex.Message,
                targetGroup = groupName
            });
        }
    }
}