using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniHelp.Api.Data;
using UniHelp.Api.DTOs;
using UniHelp.Api.Entities;

namespace UniHelp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly DataContext _context;

    public NotesController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes()
    {
        var notes = await _context.Notes
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