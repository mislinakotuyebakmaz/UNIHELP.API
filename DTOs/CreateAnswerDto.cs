using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

public class CreateAnswerDto
{
    [Required]
    public string Body { get; set; } = string.Empty;
}