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
            var members = JsonSerializer.Deserialize<List<Member>>(cached!);
            return Ok(members);
        }

        var membersFromDb = await _context.Members
            .Include(m => m.Subscriptions)
            .Include(m => m.Trainings)
            .ToListAsync();

        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(membersFromDb), TimeSpan.FromMinutes(10));

        return Ok(membersFromDb);
    }

    [HttpPost]
    public async Task<ActionResult<Member>> Create(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        await _cache.KeyDeleteAsync("members:all");

        return CreatedAtAction(nameof(GetAll), member);
    }
}