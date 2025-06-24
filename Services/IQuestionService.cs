using UniHelp.Api.DTOs;
using UniHelp.Api.Helpers;

namespace UniHelp.Api.Services;

public interface IQuestionService
{
    Task<IEnumerable<QuestionDto>> GetQuestionsAsync(QueryParameters parameters);
    Task<QuestionDto> GetQuestionByIdAsync(int id);
    Task<int> CreateQuestionAsync(CreateQuestionDto dto, int userId);
    // Diğer işlemler...
} 