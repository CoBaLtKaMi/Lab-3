using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;
using SportClubApi.Services;

namespace SportClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CacheService _cache;

    public MembersController(AppDbContext context, CacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: api/members  (с кэшем)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        var cacheKey = "members_all";
        var cached = await _cache.GetAsync<List<Member>>(cacheKey);
        if (cached != null) return cached;

        var members = await _context.Members
            .Include(m => m.Memberships)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, members, TimeSpan.FromMinutes(5));
        return members;
    }

    // GET: api/members/{id}  (с кэшем)
    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var cacheKey = $"member_{id}";
        var cached = await _cache.GetAsync<Member>(cacheKey);
        if (cached != null) return cached;

        var member = await _context.Members
            .Include(m => m.Memberships)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null) return NotFound();

        await _cache.SetAsync(cacheKey, member, TimeSpan.FromMinutes(5));
        return member;
    }

    // POST: api/members
    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // Инвалидация кэша
        await _cache.RemoveAsync("members_all");

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
    }

    // PUT: api/members/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, Member member)
    {
        if (id != member.Id) return BadRequest();

        _context.Entry(member).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MemberExists(id)) return NotFound();
            throw;
        }

        // Инвалидация кэша
        await _cache.RemoveAsync("members_all");
        await _cache.RemoveAsync($"member_{id}");

        return NoContent();
    }

    // DELETE: api/members/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null) return NotFound();

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        // Инвалидация кэша
        await _cache.RemoveAsync("members_all");
        await _cache.RemoveAsync($"member_{id}");

        return NoContent();
    }

    private bool MemberExists(int id)
    {
        return _context.Members.Any(e => e.Id == id);
    }
}