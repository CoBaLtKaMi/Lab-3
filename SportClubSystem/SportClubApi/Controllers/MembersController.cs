using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;
using SportClubApi.Services;

namespace SportClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CacheService _cache;
    private const string AllKey = "members:all";

    public MembersController(AppDbContext db, CacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cached = await _cache.GetAsync<List<Member>>(AllKey);
        if (cached != null) return Ok(cached);

        var list = await _db.Members
            .Include(m => m.Memberships)
            .ToListAsync();

        await _cache.SetAsync(AllKey, list, TimeSpan.FromMinutes(5));
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var key = $"member:{id}";
        var cached = await _cache.GetAsync<Member>(key);
        if (cached != null) return Ok(cached);

        var member = await _db.Members
            .Include(m => m.Memberships)
            .Include(m => m.WorkoutRegistrations)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null) return NotFound();

        await _cache.SetAsync(key, member, TimeSpan.FromMinutes(5));
        return Ok(member);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Member member)
    {
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Member updated)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        member.FullName = updated.FullName;
        member.Phone = updated.Phone;
        member.Email = updated.Email;
        member.BirthDate = updated.BirthDate;

        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey);
        await _cache.RemoveAsync($"member:{id}");
        return Ok(member);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey);
        await _cache.RemoveAsync($"member:{id}");
        return NoContent();
    }
}