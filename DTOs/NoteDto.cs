namespace UniHelp.Api.DTOs;

public class NoteDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorUsername { get; set; } = string.Empty; // Notu kimin oluşturduğunu göstermek için
}