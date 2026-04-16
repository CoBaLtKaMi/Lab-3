using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportClubApi.Data;
using SportClubApi.Models;

namespace SportClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkoutsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkoutsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Workouts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workout>>> GetWorkouts()
    {
        var workouts = await _context.Workouts
            .Include(w => w.Registrations)
            .ThenInclude(r => r.Member)
            .ToListAsync();

        return Ok(workouts);
    }

    // GET: api/Workouts/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Workout>> GetWorkout(int id)
    {
        var workout = await _context.Workouts
            .Include(w => w.Registrations)
            .ThenInclude(r => r.Member)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workout == null)
        {
            return NotFound();
        }

        return Ok(workout);
    }

    // POST: api/Workouts
    [HttpPost]
    public async Task<ActionResult<Workout>> PostWorkout(Workout workout)
    {
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
    }

    // PUT: api/Workouts/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutWorkout(int id, Workout workout)
    {
        if (id != workout.Id)
        {
            return BadRequest();
        }

        _context.Entry(workout).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WorkoutExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Workouts/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkout(int id)
    {
        var workout = await _context.Workouts.FindAsync(id);
        if (workout == null)
        {
            return NotFound();
        }

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Дополнительный endpoint из лабораторной (11 вариант)
    // POST: api/Workouts/{id}/register
    [HttpPost("{id}/register")]
    public async Task<ActionResult<WorkoutRegistration>> RegisterForWorkout(int id, [FromBody] int memberId)
    {
        var workout = await _context.Workouts
            .Include(w => w.Registrations)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workout == null)
            return NotFound("Тренировка не найдена");

        // Проверка лимита участников
        if (workout.Registrations.Count >= workout.MaxParticipants)
            return BadRequest("Превышен лимит участников на тренировку");

        // Проверка, что участник ещё не записан
        if (workout.Registrations.Any(r => r.MemberId == memberId))
            return BadRequest("Участник уже записан на эту тренировку");

        var registration = new WorkoutRegistration
        {
            MemberId = memberId,
            WorkoutId = id,
            RegisteredAt = DateTime.UtcNow
        };

        _context.WorkoutRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        return Ok(registration);
    }

    private bool WorkoutExists(int id)
    {
        return _context.Workouts.Any(e => e.Id == id);
    }
}