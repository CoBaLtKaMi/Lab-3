using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDatabase _cache;

    public SubscriptionsController(AppDbContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _cache = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<ActionResult<List<Subscription>>> GetAll()
    {
        const string cacheKey = "subscriptions:all";

        var cached = await _cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var items = JsonSerializer.Deserialize<List<Subscription>>(cached!);
            return Ok(items);
        }

        var itemsFromDb = await _context.Subscriptions
            .Include(s => s.Member)
            .ToListAsync();

        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(itemsFromDb), TimeSpan.FromMinutes(10));
        return Ok(itemsFromDb);
    }
}