using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;
using UniHelp.Api.Helpers;
using UniHelp.Api.Repositories;

namespace UniHelp.Api.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repo;
    public QuestionService(IQuestionRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<QuestionDto>> GetQuestionsAsync(QueryParameters parameters)
    {
        var questions = await _repo.GetQuestionsAsync(parameters);
        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            Title = q.Title,
            Body = q.Body,
            CreatedAt = q.CreatedAt,
            AuthorUsername = q.User.Username,
            Answers = q.Answers?.Select(a => new AnswerDto
            {
                Id = a.Id,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
                AuthorUsername = a.User.Username
            }).OrderBy(a => a.CreatedAt).ToList() ?? new List<AnswerDto>()
        });
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(int id)
    {
        var q = await _repo.GetQuestionByIdAsync(id);
        if (q == null) return null;
        return new QuestionDto
        {
            Id = q.Id,
            Title = q.Title,
            Body = q.Body,
            CreatedAt = q.CreatedAt,
            AuthorUsername = q.User.Username,
            Answers = q.Answers?.Select(a => new AnswerDto
            {
                Id = a.Id,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
                AuthorUsername = a.User.Username
            }).OrderBy(a => a.CreatedAt).ToList() ?? new List<AnswerDto>()
        };
    }

    public async Task<int> CreateQuestionAsync(CreateQuestionDto dto, int userId)
    {
        var question = new Question
        {
            Title = dto.Title,
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };
        await _repo.AddQuestionAsync(question);
        return question.Id;
    }
} 