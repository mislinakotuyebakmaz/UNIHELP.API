using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

public class UpdateNoteDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? FileUrl { get; set; }
}