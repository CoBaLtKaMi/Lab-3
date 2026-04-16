using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;
using SportClubApi.Services;

namespace SportClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkoutsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CacheService _cache;

    public WorkoutsController(AppDbContext context, CacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: api/workouts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workout>>> GetWorkouts()
    {
        var cacheKey = "workouts_all";
        var cached = await _cache.GetAsync<List<Workout>>(cacheKey);
        if (cached != null) return cached;

        var workouts = await _context.Workouts
            .Include(w => w.Registrations)
            .ThenInclude(r => r.Member)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, workouts, TimeSpan.FromMinutes(5));
        return workouts;
    }

    // GET: api/workouts/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Workout>> GetWorkout(int id)
    {
        var workout = await _context.Workouts
            .Include(w => w.Registrations)
            .ThenInclude(r => r.Member)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workout == null) return NotFound();

        return workout;
    }

    // POST: api/workouts
    [HttpPost]
    public async Task<ActionResult<Workout>> CreateWorkout(Workout workout)
    {
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync("workouts_all");

        return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
    }

    // PUT: api/workouts/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkout(int id, Workout workout)
    {
        if (id != workout.Id) return BadRequest();

        _context.Entry(workout).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WorkoutExists(id)) return NotFound();
            throw;
        }

        await _cache.RemoveAsync("workouts_all");
        return NoContent();
    }

    // DELETE: api/workouts/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkout(int id)
    {
        var workout = await _context.Workouts.FindAsync(id);
        if (workout == null) return NotFound();

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync("workouts_all");
        return NoContent();
    }

    // Дополнительный эндпоинт: Запись члена на тренировку
    // POST: api/workouts/{id}/register
    [HttpPost("{id}/register")]
    public async Task<IActionResult> RegisterForWorkout(int id, [FromBody] int memberId)
    {
        var workout = await _context.Workouts
            .Include(w => w.Registrations)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workout == null) return NotFound("Тренировка не найдена");

        if (workout.Registrations.Count >= workout.Capacity)
            return BadRequest("Тренировка заполнена");

        // Проверка, не записан ли уже участник
        if (workout.Registrations.Any(r => r.MemberId == memberId))
            return BadRequest("Участник уже записан на эту тренировку");

        var registration = new WorkoutRegistration
        {
            MemberId = memberId,
            WorkoutId = id
        };

        _context.WorkoutRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync("workouts_all");

        return Ok("Участник успешно записан на тренировку");
    }

    private bool WorkoutExists(int id)
    {
        return _context.Workouts.Any(e => e.Id == id);
    }
}