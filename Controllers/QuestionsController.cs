using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;
using UniHelp.Api.Helpers; // Sayfalama için eklendi

namespace UniHelp.Api.Controllers;

/// <summary>
/// Kullanıcıların soru sormasını ve mevcut soruları görüntülemesini sağlar.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly DataContext _context;

    /// <summary>
    /// QuestionsController için gerekli servisleri enjekte eder.
    /// </summary>
    /// <param name="context">Veritabanı işlemleri için DataContext.</param>
    public QuestionsController(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Yeni bir soru oluşturur.
    /// </summary>
    /// <param name="createQuestionDto">Yeni sorunun başlık ve içerik bilgileri.</param>
    /// <returns>Oluşturulan sorunun detayları.</returns>
    /// <response code="201">Soru başarıyla oluşturulduğunda döner.</response>
    /// <response code="401">Kullanıcı giriş yapmamışsa döner.</response>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), 201)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto createQuestionDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var newQuestion = new Question
        {
            Title = createQuestionDto.Title,
            Body = createQuestionDto.Body,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };
        _context.Questions.Add(newQuestion);
        await _context.SaveChangesAsync();

        var questionToReturn = new QuestionDto
        {
            Id = newQuestion.Id,
            Title = newQuestion.Title,
            Body = newQuestion.Body,
            CreatedAt = newQuestion.CreatedAt,
            AuthorUsername = user.Username,
        };
        return CreatedAtAction(nameof(GetQuestion), new { id = newQuestion.Id }, questionToReturn);
    }

    /// <summary>
    /// Sistemdeki soruları sayfalama ve filtreleme özellikleriyle listeler.
    /// </summary>
    /// <param name="queryParameters">Sayfa numarası, sayfa boyutu ve arama terimini içeren sorgu parametreleri.</param>
    /// <returns>QuestionDto listesi.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), 200)]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions([FromQuery] QueryParameters queryParameters)
    {
        IQueryable<Question> questionsQuery = _context.Questions;

        if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
        {
            questionsQuery = questionsQuery.Where(q => 
                q.Title.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                q.Body.ToLower().Contains(queryParameters.SearchTerm.ToLower())
            );
        }

        // Varsayılan sıralama (en yeni en üstte)
        questionsQuery = questionsQuery.OrderByDescending(q => q.CreatedAt);

        var questions = await questionsQuery
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Include(q => q.User)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Title = q.Title,
                Body = q.Body,
                CreatedAt = q.CreatedAt,
                AuthorUsername = q.User.Username
            }).ToListAsync();
            
        return Ok(questions);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip tek bir soruyu, tüm cevaplarıyla birlikte getirir.
    /// </summary>
    /// <param name="id">Getirilecek sorunun ID'si.</param>
    /// <returns>İstenen sorunun ve cevaplarının detayları.</returns>
    /// <response code="200">Soru bulunduğunda döner.</response>
    /// <response code="404">Belirtilen ID'ye sahip soru bulunamazsa döner.</response>
    [HttpGet("{id}", Name = "GetQuestion")]
    [ProducesResponseType(typeof(QuestionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
    {
        var question = await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Answers)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question == null) return NotFound();

        var questionDto = new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            Body = question.Body,
            CreatedAt = question.CreatedAt,
            AuthorUsername = question.User.Username,
            Answers = question.Answers.Select(a => new AnswerDto
            {
                Id = a.Id,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
                AuthorUsername = a.User.Username
            }).OrderBy(a => a.CreatedAt).ToList() // Cevapları da kendi içinde sıralayabiliriz.
        };
        return Ok(questionDto);
    }
}