using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;

namespace SportClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;

    public MembersController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/members — список всех участников
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Members
            .Include(m => m.Memberships)
            .ToListAsync();
        return Ok(list);
    }

    // GET api/members/{id} — один участник с абонементами
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var key = $"member:{id}";
        var cached = await _cache.GetAsync<Member>(key);
        if (cached != null) return Ok(cached);

        var member = await _db.Members
            .Include(m => m.Memberships)
            .Include(m => m.WorkoutRegistrations)  // ← добавь эту строку
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null) return NotFound();
        await _cache.SetAsync(key, member, TimeSpan.FromMinutes(5));
        return Ok(member);
    }

    // POST api/members — добавить нового участника
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Member member)
    {
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    // PUT api/members/{id}
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
        return Ok(member);
    }

    // DELETE api/members/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}