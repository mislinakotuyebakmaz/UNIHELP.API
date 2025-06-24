using Microsoft.EntityFrameworkCore;
using UniHelp.Api.Data;
using UniHelp.Api.Entities;
using UniHelp.Api.Helpers;

namespace UniHelp.Api.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly DataContext _context;
    public QuestionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Question>> GetQuestionsAsync(QueryParameters parameters)
    {
        IQueryable<Question> query = _context.Questions.Include(q => q.User).Include(q => q.Answers);
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(q => q.Title.ToLower().Contains(parameters.SearchTerm.ToLower()) ||
                                     q.Body.ToLower().Contains(parameters.SearchTerm.ToLower()));
        }
        query = query.OrderByDescending(q => q.CreatedAt)
                     .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                     .Take(parameters.PageSize);
        return await query.ToListAsync();
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await _context.Questions
            .Include(q => q.User)
            .Include(q => q.Answers).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task AddQuestionAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
    }
} 