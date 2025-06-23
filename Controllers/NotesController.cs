using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;
using UniHelp.Api.Helpers; // QueryParameters için eklendi

namespace UniHelp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")] // Versiyonlama eklendi
public class NotesController : ControllerBase
{
    private readonly DataContext _context;

    public NotesController(DataContext context)
    {
        _context = context;
    }

    // GET /api/v1/notes - Sayfalama, filtreleme ve sıralama eklendi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes([FromQuery] QueryParameters queryParameters)
    {
        // Temel sorguyu başlatıyoruz
        IQueryable<Note> notesQuery = _context.Notes;

        // Filtreleme (Arama terimine göre)
        if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
        {
            notesQuery = notesQuery.Where(n => 
                n.Title.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                (n.Content != null && n.Content.ToLower().Contains(queryParameters.SearchTerm.ToLower()))
            );
        }

        // Sıralama (SortBy parametresine göre)
        if (!string.IsNullOrEmpty(queryParameters.SortBy))
        {
            if (queryParameters.SortBy.Equals("date_desc", StringComparison.OrdinalIgnoreCase))
            {
                notesQuery = notesQuery.OrderByDescending(n => n.CreatedAt);
            }
            // Başka sıralama seçenekleri de eklenebilir
        }
        else
        {
            // Varsayılan sıralama (en yeni en üstte)
            notesQuery = notesQuery.OrderByDescending(n => n.CreatedAt);
        }

        // Sayfalama (Skip ve Take ile)
        var notes = await notesQuery
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Include(n => n.User) // Yazar bilgisini dahil et
            .Select(n => new NoteDto { // Sonucu DTO'ya çevir
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                FileUrl = n.FileUrl,
                CreatedAt = n.CreatedAt,
                AuthorUsername = n.User.Username
            }).ToListAsync();

        return Ok(notes);
    }

    // GET /api/v1/notes/{id} - Tek bir notu getir (Bu metod aynı kalıyor)
    [HttpGet("{id}", Name = "GetNote")]
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

    // POST /api/v1/notes - Yeni bir not oluştur (Bu metod aynı kalıyor)
    [HttpPost]
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

    // PUT /api/v1/notes/{id} - Bir notu güncelle (Bu metod aynı kalıyor)
    [HttpPut("{id}")]
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

    // DELETE /api/v1/notes/{id} - Bir notu sil (Bu metod aynı kalıyor)
    [HttpDelete("{id}")]
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