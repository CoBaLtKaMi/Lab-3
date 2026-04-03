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
    public async Task<ActionResult<IEnumerable<Training>>> GetAll()
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Training>> GetById(int id)
    {
        var item = await _context.Trainings
            .Include(t => t.Member)
            .FirstOrDefaultAsync(t => t.Id == id);

        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Training>> Create(Training training)
    {
        _context.Trainings.Add(training);
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("trainings:all");
        return CreatedAtAction(nameof(GetById), new { id = training.Id }, training);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Training training)
    {
        if (id != training.Id) return BadRequest();
        _context.Entry(training).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("trainings:all");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Trainings.FindAsync(id);
        if (item == null) return NotFound();

        _context.Trainings.Remove(item);
        await _context.SaveChangesAsync();
        await _cache.KeyDeleteAsync("trainings:all");
        return NoContent();
    }
}