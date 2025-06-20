using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

public class CreateQuestionDto
{
    [Required]
    [MinLength(10)]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}