using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/trainings")]
public class TrainingsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDatabase _cache;

    public TrainingsController(AppDbContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _cache = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<ActionResult<List<Training>>> GetAll()
    {
        const string cacheKey = "trainings:all";

        var cached = await _cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var items = JsonSerializer.Deserialize<List<Training>>(cached!);
            return Ok(items);
        }

        var itemsFromDb = await _context.Trainings
            .Include(t => t.Member)
            .ToListAsync();

        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(itemsFromDb), TimeSpan.FromMinutes(10));
        return Ok(itemsFromDb);
    }
}