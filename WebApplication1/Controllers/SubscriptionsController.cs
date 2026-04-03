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
    public async Task<ActionResult<IEnumerable<Subscription>>> GetAll()
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Subscription>> GetById(int id)
    {
        var item = await _context.Subscriptions
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == id);

        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Subscription>> Create(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("subscriptions:all");
        return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, subscription);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Subscription subscription)
    {
        if (id != subscription.Id) return BadRequest();
        _context.Entry(subscription).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("subscriptions:all");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Subscriptions.FindAsync(id);
        if (item == null) return NotFound();

        _context.Subscriptions.Remove(item);
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("subscriptions:all");
        return NoContent();
    }
}