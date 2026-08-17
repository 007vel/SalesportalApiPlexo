using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Database;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingHubLinksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TrainingHubLinksController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/traininghublinks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingHubLinkDto>>> GetAll(CancellationToken cancellationToken)
        {
            var links = await _db.TrainingHubLinks
                .AsNoTracking()
                .OrderBy(l => l.OId)
                .ToListAsync(cancellationToken);

            return Ok(links.Select(TrainingHubLinkDto.FromEntity));
        }

        // GET api/traininghublinks/5
        [HttpGet("{oId:int}")]
        public async Task<ActionResult<TrainingHubLinkDto>> GetById(int oId, CancellationToken cancellationToken)
        {
            var link = await _db.TrainingHubLinks.AsNoTracking().FirstOrDefaultAsync(l => l.OId == oId, cancellationToken);
            if (link is null)
            {
                return NotFound();
            }

            return Ok(TrainingHubLinkDto.FromEntity(link));
        }

        // POST api/traininghublinks
        [HttpPost]
        public async Task<ActionResult<TrainingHubLinkDto>> Create(TrainingHubLinkRequest request, CancellationToken cancellationToken)
        {
            var link = new TrainingHubLink
            {
                ProductVideoEnglishLink = request.ProductVideoEnglishLink,
                ProductVideoSpanishLink = request.ProductVideoSpanishLink,
                DashboardVideoEnglishLink = request.DashboardVideoEnglishLink,
                DashboardVideoSpanishLink = request.DashboardVideoSpanishLink,
                KnowledgeBaseLink = request.KnowledgeBaseLink,
                SalesLeadLink = request.SalesLeadLink
            };

            _db.TrainingHubLinks.Add(link);
            await _db.SaveChangesAsync(cancellationToken);

            var dto = TrainingHubLinkDto.FromEntity(link);
            return CreatedAtAction(nameof(GetById), new { oId = link.OId }, dto);
        }

        // PUT api/traininghublinks/5
        [HttpPut("{oId:int}")]
        public async Task<ActionResult<TrainingHubLinkDto>> Update(int oId, TrainingHubLinkRequest request, CancellationToken cancellationToken)
        {
            var link = await _db.TrainingHubLinks.FirstOrDefaultAsync(l => l.OId == oId, cancellationToken);
            if (link is null)
            {
                return NotFound();
            }

            link.ProductVideoEnglishLink = request.ProductVideoEnglishLink;
            link.ProductVideoSpanishLink = request.ProductVideoSpanishLink;
            link.DashboardVideoEnglishLink = request.DashboardVideoEnglishLink;
            link.DashboardVideoSpanishLink = request.DashboardVideoSpanishLink;
            link.KnowledgeBaseLink = request.KnowledgeBaseLink;
            link.SalesLeadLink = request.SalesLeadLink;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(TrainingHubLinkDto.FromEntity(link));
        }

        // DELETE api/traininghublinks/5
        [HttpDelete("{oId:int}")]
        public async Task<IActionResult> Delete(int oId, CancellationToken cancellationToken)
        {
            var link = await _db.TrainingHubLinks.FirstOrDefaultAsync(l => l.OId == oId, cancellationToken);
            if (link is null)
            {
                return NotFound();
            }

            _db.TrainingHubLinks.Remove(link);
            await _db.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
