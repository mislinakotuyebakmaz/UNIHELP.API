namespace UniHelp.Api.DTOs;

public class QuestionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public ICollection<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
}