using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;

namespace SportClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController : ControllerBase
{
    private readonly AppDbContext _db;
    public MembershipsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Memberships.Include(m => m.Member).ToListAsync();
        return Ok(list.Select(m => new {
            m.Id,
            m.Type,
            m.StartDate,
            m.EndDate,
            m.Price,
            m.Status,
            MemberName = m.Member!.FullName,
            DaysLeft = (int)(m.EndDate - DateTime.UtcNow).TotalDays
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _db.Memberships.Include(m => m.Member)
                         .FirstOrDefaultAsync(m => m.Id == id);
        return m == null ? NotFound() : Ok(m);
    }

    // GET api/memberships/member/{memberId} — абонементы конкретного члена
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetByMember(int memberId)
    {
        var list = await _db.Memberships
            .Where(m => m.MemberId == memberId).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Membership membership)
    {
        var member = await _db.Members.FindAsync(membership.MemberId);
        if (member == null) return BadRequest("Член клуба не найден");
        membership.StartDate = DateTime.SpecifyKind(membership.StartDate, DateTimeKind.Utc);
        membership.EndDate = DateTime.SpecifyKind(membership.EndDate, DateTimeKind.Utc);
        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Membership upd)
    {
        var m = await _db.Memberships.FindAsync(id);
        if (m == null) return NotFound();
        m.Type = upd.Type; m.Status = upd.Status;
        m.EndDate = DateTime.SpecifyKind(upd.EndDate, DateTimeKind.Utc);
        m.Price = upd.Price;
        await _db.SaveChangesAsync();
        return Ok(m);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Memberships.FindAsync(id);
        if (m == null) return NotFound();
        _db.Memberships.Remove(m);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
