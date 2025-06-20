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

[Authorize]
[ApiController]
[Route("api/questions/{questionId}/[controller]")]
public class AnswersController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<AnswersController> _logger; // Logger servisi

    // Constructor'a ILogger eklendi
    public AnswersController(
        DataContext context, 
        IHubContext<NotificationHub> hubContext, 
        ILogger<AnswersController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost]
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
}