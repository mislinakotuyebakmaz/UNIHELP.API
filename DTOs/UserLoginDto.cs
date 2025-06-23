using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

/// <summary>
/// Kullanıcı girişi için istemciden alınan verileri taşır.
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// Giriş yapacak kullanıcının adı.
    /// </summary>
    /// <example>mislina</example>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Giriş yapacak kullanıcının şifresi.
    /// </summary>
    /// <example>password123</example>
    [Required]
    public string Password { get; set; } = string.Empty;
}