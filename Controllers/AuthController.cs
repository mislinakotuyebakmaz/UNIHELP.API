using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;

namespace UniHelp.Api.Controllers;

/// <summary>
/// Kullanıcı kimlik doğrulama işlemlerini (kayıt, giriş) yönetir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// AuthController için gerekli servisleri enjekte eder.
    /// </summary>
    /// <param name="context">Veritabanı işlemleri için DataContext.</param>
    /// <param name="configuration">Yapılandırma dosyası (appsettings.json) okumaları için.</param>
    public AuthController(DataContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Yeni bir kullanıcı kaydı oluşturur.
    /// </summary>
    /// <param name="request">Kullanıcının kayıt için verdiği kullanıcı adı, email ve şifre bilgileri.</param>
    /// <returns>Oluşturulan kullanıcı nesnesi.</returns>
    /// <response code="200">Kullanıcı başarıyla oluşturulduğunda döner.</response>
    /// <response code="400">Belirtilen kullanıcı adı veya email zaten mevcutsa döner.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower() || u.Email.ToLower() == request.Email.ToLower()))
        {
            return BadRequest("Username or Email already exists.");
        }

        using var hmac = new HMACSHA512();
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password))
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    /// <summary>
    /// Mevcut bir kullanıcının sisteme giriş yapmasını sağlar ve bir JWT döndürür.
    /// </summary>
    /// <param name="request">Kullanıcının giriş için verdiği kullanıcı adı ve şifre.</param>
    /// <returns>Başarılı girişte bir JWT (JSON Web Token).</returns>
    /// <response code="200">Giriş başarılı olduğunda token döner.</response>
    /// <response code="401">Kullanıcı adı veya şifre yanlışsa döner.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(string), 401)]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
        if (user == null)
        {
            return Unauthorized("Invalid Credentials.");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
        if (!computedHash.SequenceEqual(user.PasswordHash))
        {
            return Unauthorized("Invalid Credentials.");
        }

        string token = CreateToken(user);
        return Ok(new { token = token });
    }

    /// <summary>
    /// Yetkilendirmenin çalışıp çalışmadığını test etmek için korumalı bir endpoint.
    /// </summary>
    /// <remarks>
    /// Bu endpoint'e sadece geçerli bir JWT ile (Authorization: Bearer [token]) erişilebilir.
    /// </remarks>
    /// <returns>Giriş yapmış kullanıcının bilgilerini içeren bir selamlama mesajı.</returns>
    /// <response code="200">Kullanıcı yetkiliyse döner.</response>
    /// <response code="401">Kullanıcı giriş yapmamışsa veya token geçersizse döner.</response>
    [HttpGet("test-auth")]
    [Authorize]
    public IActionResult TestAuth()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        return Ok($"Merhaba {username}! (ID: {userId}). Bu korumalı bir mesajdır.");
    }

    /// <summary>
    /// Belirtilen kullanıcı için bir JWT oluşturur.
    /// </summary>
    /// <param name="user">Token oluşturulacak kullanıcı nesnesi.</param>
    /// <returns>Oluşturulan token'ın string hali.</returns>
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
        if (appSettingsToken is null)
            throw new Exception("AppSettings:Token anahtarı appsettings.json dosyasında bulunamadı!");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}