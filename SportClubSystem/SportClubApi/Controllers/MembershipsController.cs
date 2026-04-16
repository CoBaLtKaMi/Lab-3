using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;

namespace SportClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MembershipsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MembershipsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/memberships
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Membership>>> GetMemberships()
    {
        var memberships = await _context.Memberships
            .Include(m => m.Member)
            .ToListAsync();

        return memberships;
    }

    // GET: api/memberships/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Membership>> GetMembership(int id)
    {
        var membership = await _context.Memberships
            .Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (membership == null) return NotFound();

        return membership;
    }

    // POST: api/memberships
    [HttpPost]
    public async Task<ActionResult<Membership>> CreateMembership(Membership membership)
    {
        // Проверка существования члена клуба
        if (!await _context.Members.AnyAsync(m => m.Id == membership.MemberId))
            return BadRequest("Член клуба с таким ID не существует");

        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMembership), new { id = membership.Id }, membership);
    }

    // PUT: api/memberships/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMembership(int id, Membership membership)
    {
        if (id != membership.Id) return BadRequest();

        _context.Entry(membership).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MembershipExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/memberships/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMembership(int id)
    {
        var membership = await _context.Memberships.FindAsync(id);
        if (membership == null) return NotFound();

        _context.Memberships.Remove(membership);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MembershipExists(int id)
    {
        return _context.Memberships.Any(e => e.Id == id);
    }
}