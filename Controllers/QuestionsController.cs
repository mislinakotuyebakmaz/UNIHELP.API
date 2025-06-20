using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;

namespace UniHelp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly DataContext _context;

    public QuestionsController(DataContext context)
    {
        _context = context;
    }

    [HttpPost]
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions()
    {
        var questions = await _context.Questions
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

    [HttpGet("{id}", Name = "GetQuestion")]
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
            }).ToList()
        };
        return Ok(questionDto);
    }
}