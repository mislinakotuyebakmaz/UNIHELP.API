using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;
using UniHelp.Api.Helpers;

namespace UniHelp.Api.Controllers;

/// <summary>
/// Ders notlarını oluşturma, okuma, güncelleme ve silme (CRUD) işlemlerini yönetir.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class NotesController : ControllerBase
{
    private readonly DataContext _context;

    /// <summary>
    /// NotesController için gerekli servisleri enjekte eder.
    /// </summary>
    /// <param name="context">Veritabanı işlemleri için DataContext.</param>
    public NotesController(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Sistemdeki notları sayfalama, filtreleme ve sıralama özellikleriyle listeler.
    /// </summary>
    /// <param name="queryParameters">Sayfa numarası, sayfa boyutu, arama terimi ve sıralama kriterlerini içeren sorgu parametreleri.</param>
    /// <returns>NoteDto listesi.</returns>
    /// <remarks>
    /// Örnek istek: GET /api/v1/notes?pageNumber=1&pageSize=10&searchTerm=asp.net&sortBy=date_desc
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NoteDto>), 200)]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes([FromQuery] QueryParameters queryParameters)
    {
        IQueryable<Note> notesQuery = _context.Notes;

        if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
        {
            notesQuery = notesQuery.Where(n => 
                n.Title.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                (n.Content != null && n.Content.ToLower().Contains(queryParameters.SearchTerm.ToLower()))
            );
        }

        if (!string.IsNullOrEmpty(queryParameters.SortBy) && queryParameters.SortBy.Equals("date_desc", StringComparison.OrdinalIgnoreCase))
        {
            notesQuery = notesQuery.OrderByDescending(n => n.CreatedAt);
        }
        else
        {
            notesQuery = notesQuery.OrderByDescending(n => n.CreatedAt);
        }

        var notes = await notesQuery
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Include(n => n.User)
            .Select(n => new NoteDto {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                FileUrl = n.FileUrl,
                CreatedAt = n.CreatedAt,
                AuthorUsername = n.User.Username
            }).ToListAsync();

        return Ok(notes);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip tek bir notu getirir.
    /// </summary>
    /// <param name="id">Getirilecek notun ID'si.</param>
    /// <returns>İstenen notun detayları.</returns>
    /// <response code="200">Not bulunduğunda döner.</response>
    /// <response code="404">Belirtilen ID'ye sahip not bulunamazsa döner.</response>
    [HttpGet("{id}", Name = "GetNote")]
    [ProducesResponseType(typeof(NoteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<NoteDto>> GetNote(int id)
    {
        var note = await _context.Notes
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
        if (note == null) return NotFound();

        var noteToReturn = new NoteDto {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            FileUrl = note.FileUrl,
            CreatedAt = note.CreatedAt,
            AuthorUsername = note.User.Username
        };
        return Ok(noteToReturn);
    }

    /// <summary>
    /// Yeni bir not oluşturur.
    /// </summary>
    /// <param name="createNoteDto">Yeni notun başlık ve içerik bilgileri.</param>
    /// <returns>Oluşturulan notun detayları.</returns>
    /// <response code="201">Not başarıyla oluşturulduğunda döner.</response>
    /// <response code="401">Kullanıcı giriş yapmamışsa döner.</response>
    [HttpPost]
    [ProducesResponseType(typeof(NoteDto), 201)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto createNoteDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var newNote = new Note
        {
            Title = createNoteDto.Title,
            Content = createNoteDto.Content,
            FileUrl = createNoteDto.FileUrl,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };
        _context.Notes.Add(newNote);
        await _context.SaveChangesAsync();

        var noteToReturn = new NoteDto {
            Id = newNote.Id,
            Title = newNote.Title,
            Content = newNote.Content,
            FileUrl = newNote.FileUrl,
            CreatedAt = newNote.CreatedAt,
            AuthorUsername = user.Username
        };
        return CreatedAtAction(nameof(GetNote), new { id = newNote.Id }, noteToReturn);
    }

    /// <summary>
    /// Mevcut bir notu günceller.
    /// </summary>
    /// <param name="id">Güncellenecek notun ID'si.</param>
    /// <param name="updateNoteDto">Notun yeni başlık ve içerik bilgileri.</param>
    /// <returns>İçerik yok (No Content) yanıtı.</returns>
    /// <response code="204">Güncelleme başarılı olduğunda döner.</response>
    /// <response code="403">Kullanıcı, notun sahibi değilse döner (Forbidden).</response>
    /// <response code="404">Belirtilen ID'ye sahip not bulunamazsa döner.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateNote(int id, UpdateNoteDto updateNoteDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);

        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();
        if (note.UserId != userId) return Forbid();

        note.Title = updateNoteDto.Title;
        note.Content = updateNoteDto.Content;
        note.FileUrl = updateNoteDto.FileUrl;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Mevcut bir notu siler.
    /// </summary>
    /// <param name="id">Silinecek notun ID'si.</param>
    /// <returns>İçerik yok (No Content) yanıtı.</returns>
    /// <response code="204">Silme başarılı olduğunda döner.</response>
    /// <response code="403">Kullanıcı, notun sahibi değilse döner (Forbidden).</response>
    /// <response code="404">Belirtilen ID'ye sahip not bulunamazsa döner.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);
        
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();
        if (note.UserId != userId) return Forbid();

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}