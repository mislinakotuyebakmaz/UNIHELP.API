using UniHelp.Api.Entities;
using UniHelp.Api.Helpers;

namespace UniHelp.Api.Repositories;

public interface IQuestionRepository
{
    Task<IEnumerable<Question>> GetQuestionsAsync(QueryParameters parameters);
    Task<Question> GetQuestionByIdAsync(int id);
    Task AddQuestionAsync(Question question);
    // Diğer işlemler...
} 