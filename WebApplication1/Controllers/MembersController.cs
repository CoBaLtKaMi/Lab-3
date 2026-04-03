using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDatabase _cache;

    public MembersController(AppDbContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _cache = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<ActionResult<List<Member>>> GetAll()
    {
        const string cacheKey = "members:all";

        var cached = await _cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var membersFromCache = JsonSerializer.Deserialize<List<Member>>(cached!);
            return Ok(membersFromCache);
        }

        var membersFromDb = await _context.Members
            .Include(m => m.Subscriptions)
            .Include(m => m.Trainings)
            .ToListAsync();

        await _cache.StringSetAsync(cacheKey,
            JsonSerializer.Serialize(membersFromDb),
            TimeSpan.FromMinutes(10));

        return Ok(membersFromDb);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetById(int id)
    {
        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .Include(m => m.Trainings)
            .FirstOrDefaultAsync(m => m.Id == id);

        return member == null ? NotFound() : Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<Member>> Create(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        await _cache.KeyDeleteAsync("members:all");

        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Member member)
    {
        if (id != member.Id) return BadRequest();

        _context.Entry(member).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("members:all");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null) return NotFound();

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("members:all");
        return NoContent();
    }
}