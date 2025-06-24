using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniHelp.Api.DTOs;
using UniHelp.Api.Helpers;
using UniHelp.Api.Services;

namespace UniHelp.Api.Controllers;

/// <summary>
/// Kullanıcıların soru sormasını ve mevcut soruları görüntülemesini sağlar.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _service;

    /// <summary>
    /// QuestionsController için gerekli servisleri enjekte eder.
    /// </summary>
    /// <param name="service">Soru işlemleri için IQuestionService.</param>
    public QuestionsController(IQuestionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Yeni bir soru oluşturur.
    /// </summary>
    /// <param name="createQuestionDto">Yeni sorunun başlık ve içerik bilgileri.</param>
    /// <returns>İşlemin başarılı olduğunu ve oluşturulan sorunun ID'sini içeren bir mesaj.</returns>
    /// <response code="200">Soru başarıyla oluşturulduğunda döner.</response>
    /// <response code="401">Kullanıcı giriş yapmamışsa döner.</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponse<object>>> CreateQuestion(CreateQuestionDto createQuestionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object>("Geçersiz veri gönderildi."));
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) return Unauthorized(new ApiResponse<object>("Kullanıcı bulunamadı."));
        var userId = int.Parse(userIdString);
        var questionId = await _service.CreateQuestionAsync(createQuestionDto, userId);
        return Ok(new ApiResponse<object>(new { message = "Soru başarıyla oluşturuldu", questionId }));
    }

    /// <summary>
    /// Sistemdeki soruları sayfalama ve filtreleme özellikleriyle listeler.
    /// </summary>
    /// <param name="queryParameters">Sayfa numarası, sayfa boyutu ve arama terimini içeren sorgu parametreleri.</param>
    /// <returns>QuestionDto listesi.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<QuestionDto>>>> GetQuestions([FromQuery] QueryParameters queryParameters)
    {
        var result = await _service.GetQuestionsAsync(queryParameters);
        return Ok(new ApiResponse<IEnumerable<QuestionDto>>(result));
    }

    /// <summary>
    /// Belirtilen ID'ye sahip tek bir soruyu, tüm cevaplarıyla birlikte getirir.
    /// </summary>
    /// <param name="id">Getirilecek sorunun ID'si.</param>
    /// <returns>İstenen sorunun ve cevaplarının detayları.</returns>
    /// <response code="200">Soru bulunduğunda döner.</response>
    /// <response code="404">Belirtilen ID'ye sahip soru bulunamazsa döner.</response>
    [HttpGet("{id}", Name = "GetQuestion")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(QuestionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ApiResponse<QuestionDto>>> GetQuestion(int id)
    {
        var result = await _service.GetQuestionByIdAsync(id);
        if (result == null) return NotFound(new ApiResponse<QuestionDto>("Soru bulunamadı"));
        return Ok(new ApiResponse<QuestionDto>(result));
    }
}