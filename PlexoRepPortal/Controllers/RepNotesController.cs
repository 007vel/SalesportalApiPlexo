using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepNotesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RepNotesController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/repnotes — creates or replaces the one note on file for a rep+kind pair.
        [HttpPost]
        public async Task<ActionResult<RepNoteDto>> Upsert(RepNoteRequest request, CancellationToken cancellationToken)
        {
            var repExists = await _db.Reps.AnyAsync(r => r.RepId == request.RepId, cancellationToken);
            if (!repExists)
            {
                return NotFound($"Rep with RepId '{request.RepId}' was not found.");
            }

            var existing = await _db.RepNotes.FirstOrDefaultAsync(
                n => n.RepId == request.RepId && n.Kind == request.Kind, cancellationToken);
            var now = DateTime.UtcNow;

            if (existing is null)
            {
                existing = new RepNote { RepId = request.RepId, Kind = request.Kind, CreatedAt = now };
                _db.RepNotes.Add(existing);
            }

            existing.Text = request.Text;
            existing.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(RepNoteDto.FromEntity(existing));
        }

        // GET api/repnotes/rep/1000 — every note on file for a rep (both the Admin and Shared kind, if set).
        [HttpGet("rep/{repId}")]
        public async Task<ActionResult<IEnumerable<RepNoteDto>>> GetByRep(string repId, CancellationToken cancellationToken)
        {
            var repExists = await _db.Reps.AnyAsync(r => r.RepId == repId, cancellationToken);
            if (!repExists)
            {
                return NotFound($"Rep with RepId '{repId}' was not found.");
            }

            var notes = await _db.RepNotes
                .AsNoTracking()
                .Where(n => n.RepId == repId)
                .OrderBy(n => n.Kind)
                .ToListAsync(cancellationToken);

            return Ok(notes.Select(RepNoteDto.FromEntity));
        }

        // PUT api/repnotes/5 — updates the text of an existing note. RepId/Kind stay as they are.
        [HttpPut("{oId:int}")]
        public async Task<ActionResult<RepNoteDto>> Update(int oId, RepNoteUpdateRequest request, CancellationToken cancellationToken)
        {
            var note = await _db.RepNotes.FirstOrDefaultAsync(n => n.OId == oId, cancellationToken);
            if (note is null)
            {
                return NotFound();
            }

            note.Text = request.Text;
            note.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(RepNoteDto.FromEntity(note));
        }

        // DELETE api/repnotes/5 — removes the note entirely (the card goes back to "No notes yet").
        [HttpDelete("{oId:int}")]
        public async Task<IActionResult> Delete(int oId, CancellationToken cancellationToken)
        {
            var note = await _db.RepNotes.FirstOrDefaultAsync(n => n.OId == oId, cancellationToken);
            if (note is null)
            {
                return NotFound();
            }

            _db.RepNotes.Remove(note);
            await _db.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
