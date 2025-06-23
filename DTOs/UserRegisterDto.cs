using System.ComponentModel.DataAnnotations;

namespace UniHelp.Api.DTOs;

/// <summary>
/// Yeni bir kullanıcı kaydı için istemciden alınan verileri taşır.
/// </summary>
public class UserRegisterDto
{
    /// <summary>
    /// Kullanıcının benzersiz adı.
    /// </summary>
    /// <example>mislina</example>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının geçerli e-posta adresi.
    /// </summary>
    /// <example>mislina@test.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının belirlediği şifre. Minimum 6 karakter olmalıdır.
    /// </summary>
    /// <example>password123</example>
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}